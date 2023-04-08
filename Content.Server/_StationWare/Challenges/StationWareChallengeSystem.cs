using System.Linq;
using Content.Server._StationWare.ChallengeOverlay;
using Content.Server.Administration.Commands;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Server.Spawners.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using JetBrains.Annotations;
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
    [Dependency] private readonly ChallengeOverlaySystem _overlay = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeEffects();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);

        _consoleHost.RegisterCommand("startchallenge", "Starts the specified challenge", "startchallenge <prototype ID>",
            StartChallengeCommand,
            StartChallengeCommandCompletions);
    }

    /// <remarks>
    /// if someone spawns midround, just add them to every active challenge
    /// </remarks>
    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        var netuserId = ev.Player.UserId;
        foreach (var challenge in EntityQuery<StationWareChallengeComponent>())
        {
            if (!challenge.Completions.ContainsKey(netuserId))
                challenge.Completions[netuserId] = null;
        }
    }

    public EntityUid StartChallenge(ChallengePrototype challengePrototype, float challengeTimeMultiplier = 1)
    {
        var uid = Spawn(null, MapCoordinates.Nullspace);
        var challengeComp = AddComp<StationWareChallengeComponent>(uid);
        var duration = challengePrototype.Duration * (challengePrototype.InvertSpeedup
            ? 1f + (1f - challengeTimeMultiplier)
            : challengeTimeMultiplier);
        var startDelay = challengePrototype.StartDelay * challengeTimeMultiplier;
        challengeComp.StartTime = _timing.CurTime + startDelay;
        challengeComp.EndTime = _timing.CurTime + duration + startDelay;
        challengeComp.WinByDefault = challengePrototype.WinByDefault;
        var participants = GetParticipants();

        var announcement = Loc.GetString(challengePrototype.Announcement);
        _overlay.BroadcastText(announcement, true, Color.Fuchsia);
        _chat.DispatchGlobalAnnouncement(announcement, announcementSound: challengePrototype.AnnouncementSound, colorOverride: Color.Fuchsia);

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

        foreach (var id in participants.Keys)
        {
            challengeComp.Completions.Add(id, null);
        }

        return uid;
    }

    public void EndChallenge(EntityUid uid, StationWareChallengeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var beforeEv = new BeforeChallengeEndEvent(GetEntitiesFromNetUserIds(component.Completions.Keys).ToList(), component);
        RaiseLocalEvent(uid, ref beforeEv);

        _overlay.BroadcastText(string.Empty, true, Color.White);

        foreach (var (player, completion) in component.Completions)
        {
            if (completion != null)
                continue;
            if (!_player.TryGetSessionById(player, out var session) ||
                session.AttachedEntity is not { } attachedEntity ||
                !SetPlayerChallengeState(attachedEntity, uid, component.WinByDefault, component))
            {
                // Automatically make players fail if they're not in the game
                component.Completions[player] = false;
            }
        }

        Dictionary<NetUserId, bool> finalCompletions = new();
        foreach (var (player, completion)  in component.Completions)
        {
            if (completion == null)
            {
                Logger.Error($"Null completion for challenge {ToPrettyString(uid)}: {player}, {player.UserId}");
                continue;
            }

            finalCompletions.Add(player, completion.Value);
        }

        var ev = new ChallengeEndEvent(GetEntitiesFromNetUserIds(component.Completions.Keys).ToList(), finalCompletions, uid, component);
        RaiseLocalEvent(uid, ref ev, true);
        RespawnPlayers(component.Completions.Keys.ToHashSet());
        Del(uid);
    }

    /// <summary>
    /// Ends the challenge early if everyone has already completed it.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    [PublicAPI]
    public bool TryEndChallengeEarly(EntityUid uid, StationWareChallengeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Completions.Values.Any(v => v == null))
            return false;

        // Queues the challenge to end on the next tick.
        component.EndTime = TimeSpan.Zero;
        return true;
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

        if (win)
        {
            _overlay.BroadcastText(Loc.GetString("overlay-won"), true, Color.Green, actor.PlayerSession);
        }
        else
        {
            _overlay.BroadcastText(Loc.GetString("overlay-lost"), true, Color.Red, actor.PlayerSession);
        }

        var ev = new PlayerChallengeStateSetEvent(challengeEnt, actor.PlayerSession, win);
        RaiseLocalEvent(uid, ref ev);
        RaiseLocalEvent(challengeEnt, ref ev, true);
        TryEndChallengeEarly(challengeEnt, component);
        return true;
    }

    private Dictionary<NetUserId, EntityUid> GetParticipants()
    {
        var participants = new Dictionary<NetUserId, EntityUid>();
        var enumerator = EntityQueryEnumerator<ActorComponent, MobStateComponent>();
        while (enumerator.MoveNext(out var uid, out var actor, out var mobState))
        {
            if (HasComp<GhostComponent>(uid))
                continue;

            if (_mobState.IsDead(uid, mobState))
                continue;

            if (participants.ContainsKey(actor.PlayerSession.UserId))
                continue;

            participants.Add(actor.PlayerSession.UserId, uid);
        }
        return participants;
    }

    public void RespawnPlayers(HashSet<NetUserId> players)
    {
        if (!players.Any())
            return;

        HashSet<IPlayerSession> sessions = new();
        foreach (var id in players)
        {
            if (_player.TryGetSessionById(id, out var session))
                sessions.Add(session);
        }
        RespawnPlayers(sessions);
    }

    public void RespawnPlayers(HashSet<IPlayerSession> players)
    {
        if (!players.Any())
            return;

        var validSpawns = EntityQuery<SpawnPointComponent, TransformComponent>()
            .Where(p => p.Item1.SpawnType == SpawnPointType.LateJoin)
            .Select(p => p.Item2).ToList();

        foreach (var session in players)
        {
            if (session.AttachedEntity is not { } entity ||
                HasComp<GhostComponent>(entity) || // are you a ghostie?
                !HasComp<MobStateComponent>(entity)) // or did you get your ass gibbed
            {
                _gameTicker.SpawnPlayer(session, EntityUid.Invalid, null, false, false);
                continue;
            }

            RejuvenateCommand.PerformRejuvenate(entity);

            var xform = Transform(entity);
            if (xform.GridUid == null)
            {
                var spawn = _random.Pick(validSpawns);
                _transform.SetCoordinates(entity, xform, spawn.Coordinates);
            }
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

            if (HasComp<GhostComponent>(ent))
                continue;

            if (_mobState.IsIncapacitated(ent))
                continue;

            yield return ent;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<StationWareChallengeComponent>();
        while (enumerator.MoveNext(out var uid, out var challenge))
        {
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
