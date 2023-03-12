using System.Linq;
using Content.Server.Administration.Commands;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Server.Spawners.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Network;
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
    [Dependency] private readonly IPlayerManager _player = default!;
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
        var participants = GetParticipants(); // get all of the players for this challenge

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

        foreach (var session in participants.Keys)
        {
            challengeComp.Completions.Add(session.UserId, null);
        }

        var announcement = Loc.GetString(challengePrototype.Announcement);
        _chat.DispatchGlobalAnnouncement(announcement, announcementSound: challengePrototype.AnnouncementSound, colorOverride: Color.Fuchsia);
        return uid;
    }

    public void EndChallenge(EntityUid uid, StationWareChallengeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var beforeEv = new BeforeChallengeEndEvent(GetEntitiesFromNetUserIds(component.Completions.Keys).ToList(), component);
        RaiseLocalEvent(uid, ref beforeEv);

        foreach (var (player, completion) in component.Completions)
        {
            if (completion != null)
                continue;
            if (!_player.TryGetSessionById(player, out var session) || session.AttachedEntity is not { } attachedEntity)
                continue;
            if (!SetPlayerChallengeState(attachedEntity, uid, component.WinByDefault, component))
            {
                component.Completions[player] = component.WinByDefault;
            }
        }

        Dictionary<NetUserId, bool> finalCompletions = new();
        foreach (var (player, completion)  in component.Completions)
        {
            finalCompletions.Add(player, completion!.Value);
        }

        var ev = new ChallengeEndEvent(GetEntitiesFromNetUserIds(component.Completions.Keys).ToList(), finalCompletions, uid, component);
        RaiseLocalEvent(uid, ref ev, true);
        RespawnPlayers(component.Completions.Keys);
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

        var id = actor.PlayerSession.UserId;

        if (!component.Completions.ContainsKey(id))
            return false;

        if (component.Completions[id] != null)
            return false;

        component.Completions[id] = win;

        var effect = win
            ? component.WinEffectPrototypeId
            : component.LoseEffectPrototypeId;
        var effectEnt = Spawn(effect, new EntityCoordinates(uid, 0, 0));
        EnsureComp<ChallengeStateEffectComponent>(effectEnt).Challenge = challengeEnt;

        var ev = new PlayerChallengeStateSetEvent(challengeEnt, actor.PlayerSession, win);
        RaiseLocalEvent(uid, ref ev);
        RaiseLocalEvent(challengeEnt, ref ev, true);
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

    public void RespawnPlayers(IEnumerable<NetUserId> players)
    {
        foreach (var id in players)
        {
            RespawnPlayer(id);
        }
    }

    public void RespawnPlayer(NetUserId id)
    {
        if (_player.TryGetSessionById(id, out var session))
            RespawnPlayer(session);
    }

    public void RespawnPlayer(IPlayerSession session)
    {
        if (session.AttachedEntity is not { } entity ||
            HasComp<GhostComponent>(entity) || // are you a ghostie?
            !HasComp<MobStateComponent>(entity)) // or did you get your ass gibbed
        {
            _gameTicker.SpawnPlayer(session, EntityUid.Invalid, null, false, false);
            return;
        }

        RejuvenateCommand.PerformRejuvenate(entity);

        var xform = Transform(entity);
        if (xform.GridUid == null)
        {
            var validSpawns = EntityQuery<SpawnPointComponent>()
                .Where(s => s.SpawnType == SpawnPointType.LateJoin)
                .Select(s => Transform(s.Owner)).ToList();
            var spawn = _random.Pick(validSpawns);
            _transform.SetCoordinates(xform, spawn.Coordinates);
        }
    }

    public IEnumerable<EntityUid> GetEntitiesFromNetUserIds(IEnumerable<NetUserId> sessions)
    {
        foreach (var id in sessions)
        {
            if (!_player.TryGetSessionById(id, out var session))
                continue;

            if (session.AttachedEntity is not {} ent)
                continue;
            yield return ent;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var challenge in EntityQuery<StationWareChallengeComponent>())
        {
            var uid = challenge.Owner;

            if (challenge.StartTime != null && _timing.CurTime >= challenge.StartTime)
            {
                var ev = new ChallengeStartEvent(GetEntitiesFromNetUserIds(challenge.Completions.Keys).ToList(), uid, challenge);
                RaiseLocalEvent(uid, ref ev, true);
                challenge.StartTime = null;
            }

            if (challenge.EndTime != null && _timing.CurTime >= challenge.EndTime)
            {
                EndChallenge(uid, challenge);
            }
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
public readonly record struct ChallengeEndEvent(List<EntityUid> Players, Dictionary<NetUserId, bool> Completions, EntityUid Challenge, StationWareChallengeComponent Component);

/// <summary>
/// Raised when a player wins/loses a challenge
/// </summary>
/// <param name="Challenge"></param>
/// <param name="Player"></param>
/// <param name="Won"></param>
[ByRefEvent]
public readonly record struct PlayerChallengeStateSetEvent(EntityUid Challenge, IPlayerSession Player, bool Won);
