using System.Linq;
using Content.Server.Administration.Commands;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.Challenges;

public sealed partial class StationWareChallengeSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeEffects();

        _consoleHost.RegisterCommand("startchallenge", "Starts the specified challenge", "startchallenge <prototype ID>",
            StartChallengeCommand,
            StartChallengeCommandCompletions);
    }

    public EntityUid StartChallenge(ChallengePrototype challengePrototype)
    {
        var uid = Spawn(null, MapCoordinates.Nullspace);
        var challengeComp = AddComp<StationWareChallengeComponent>(uid);
        challengeComp.StartTime = _timing.CurTime + challengePrototype.StartDelay;
        challengeComp.EndTime = _timing.CurTime + challengePrototype.Duration + challengePrototype.StartDelay; // time modifiers?
        challengeComp.WinByDefault = challengePrototype.WinByDefault;
        challengeComp.Participants = GetParticipants(); // get all of the players for this challenge

        // copy all our modifiers from the challenge component to the entity
        foreach (var (name, entry) in challengePrototype.ChallengeModifiers)
        {
            var reg = _componentFactory.GetRegistration(name);
            var comp = (Component) _componentFactory.GetComponent(reg);
            comp.Owner = uid;
            var temp = (object) comp;
            _serialization.CopyTo(entry.Component, ref temp);
            EntityManager.AddComponent(uid, (Component) temp!, true);
        }

        foreach (var session in challengeComp.Participants.Keys)
        {
            challengeComp.Completions.Add(session, null);
        }

        var announcement = Loc.GetString(challengePrototype.Announcement);
        _chat.DispatchGlobalAnnouncement(announcement, announcementSound: challengePrototype.AnnouncementSound, colorOverride: Color.Fuchsia);
        return uid;
    }

    public void EndChallenge(EntityUid uid, StationWareChallengeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var beforeEv = new BeforeChallengeEndEvent(component.Participants.Values.ToList(), component);
        RaiseLocalEvent(uid, ref beforeEv);

        foreach (var (player, completion) in component.Completions)
        {
            if (completion != null)
                continue;
            if (player.AttachedEntity == null ||
                !SetPlayerChallengeState(player.AttachedEntity.Value, uid, component.WinByDefault, component))
            {
                component.Completions[player] = component.WinByDefault;
            }
        }

        Dictionary<IPlayerSession, bool> finalCompletions = new();
        foreach (var (player, completion)  in component.Completions)
        {
            finalCompletions.Add(player, completion!.Value);
        }

        var ev = new ChallengeEndEvent(component.Participants.Values.ToList(), finalCompletions, uid, component);
        RaiseLocalEvent(uid, ref ev, true);
        RespawnPlayers(component.Participants.Keys.ToList());
        Del(uid);
    }

    /// <summary>
    /// Sets a player as winning a challenge.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="challengeEnt"></param>
    /// <param name="win"></param>
    /// <param name="component"></param>
    /// <param name="actor"></param>
    public bool SetPlayerChallengeState(EntityUid uid,
        EntityUid challengeEnt,
        bool win,
        StationWareChallengeComponent? component = null,
        ActorComponent? actor = null)
    {
        if (!Resolve(uid, ref actor, false) || !Resolve(challengeEnt, ref component))
            return false;

        if (!component.Participants.ContainsKey(actor.PlayerSession))
            return false;

        if (component.Completions[actor.PlayerSession] != null)
            return false;

        component.Completions[actor.PlayerSession] = win;

        var xform = Transform(uid);
        var effect = win
            ? component.WinEffectPrototypeId
            : component.LoseEffectPrototypeId;
        var effectEnt = Spawn(effect, xform.Coordinates);
        _transform.SetParent(effectEnt, uid);
        EnsureComp<ChallengeStateEffectComponent>(effectEnt).Challenge = challengeEnt;
        return true;
    }

    private Dictionary<IPlayerSession, EntityUid> GetParticipants()
    {
        var participants = new Dictionary<IPlayerSession, EntityUid>();
        var query = EntityQuery<ActorComponent, MobStateComponent>().ToList();
        _random.Shuffle(query);
        foreach (var (actor, mobState) in query)
        {
            var ent = actor.Owner;
            if (_mobState.IsDead(ent, mobState))
                continue;

            if (participants.ContainsKey(actor.PlayerSession))
                continue;

            participants.Add(actor.PlayerSession, ent);
        }
        return participants;
    }

    private void RespawnPlayers(List<IPlayerSession> players)
    {
        foreach (var session in players)
        {
            // they belong to the void now
            if (session.AttachedEntity is not { } entity)
                continue;

            if (HasComp<GhostComponent>(entity) || // are you a ghostie?
                !HasComp<MobStateComponent>(entity)) // or did you get your ass gibbed
            {
                _gameTicker.SpawnPlayer(session, EntityUid.Invalid, null, false, false);
                continue;
            }

            RejuvenateCommand.PerformRejuvenate(entity);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var challenge in EntityQuery<StationWareChallengeComponent>())
        {
            var uid = challenge.Owner;

            if (challenge.StartTime != null &&
                _timing.CurTime >= challenge.StartTime)
            {
                var ev = new ChallengeStartEvent(challenge.Participants.Values.ToList(), uid, challenge);
                RaiseLocalEvent(uid, ref ev, true);
                challenge.StartTime = null;
            }


            if (_timing.CurTime < challenge.EndTime)
                continue;

            EndChallenge(uid, challenge);
        }
    }
}

/// <summary>
/// Event raised once the challenge begins and logic can start.
/// </summary>
/// <param name="Players"></param>
/// <param name="Challenge"></param>
[ByRefEvent]
public readonly record struct ChallengeStartEvent(List<EntityUid> Players, EntityUid Challenge, StationWareChallengeComponent Component);

/// <summary>
/// Event raised before the winners are checked but at the end of the challenge.
/// </summary>
/// <param name="Players"></param>
[ByRefEvent]
public readonly record struct BeforeChallengeEndEvent(List<EntityUid> Players, StationWareChallengeComponent Component);

/// <summary>
/// Event raised at the end of a challenge.
/// Used for cleanup and checking winners.
/// </summary>
/// <param name="Players"></param>
/// <param name="Completions"></param>
/// <param name="Challenge"></param>
[ByRefEvent]
public readonly record struct ChallengeEndEvent(List<EntityUid> Players, Dictionary<IPlayerSession, bool> Completions, EntityUid Challenge, StationWareChallengeComponent Component);
