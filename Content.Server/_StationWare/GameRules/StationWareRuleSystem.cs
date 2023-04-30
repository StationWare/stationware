using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._StationWare.ChallengeOverlay;
using Content.Server._StationWare.Challenges;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server._StationWare.Points;
using Content.Server.Chat.Managers;
using Content.Server.CombatMode;
using Content.Server.Damage.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Hands.Systems;
using Content.Shared.CombatMode;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.GameRules;

public sealed class StationWareRuleSystem : GameRuleSystem<StationWareRuleComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    [Dependency] private readonly GodmodeSystem _godmode = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private readonly ChallengeOverlaySystem _overlay = default!;
    [Dependency] private readonly PointSystem _point = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        _overlay.BroadcastText("", false, Color.Green);
    }

    private void OnChallengeEnd(ref ChallengeEndEvent ev)
    {
        var query = EntityQueryEnumerator<StationWareRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var ware, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                return;

            if (ev.Challenge != ware.CurrentChallenge)
                return;

            if (ware.ChallengeCount >= ware.TotalChallenges)
            {
                if (CheckForTies(out var tiedPlayers))
                {
                    // there's a tie!
                    foreach (var player in tiedPlayers)
                    {
                        if (_playerManager.TryGetSessionById(player, out var playerSession))
                        {
                            if (playerSession.AttachedEntity == null)
                                return;

                            var entity = playerSession.AttachedEntity.Value;

                            AddComp<TiebreakerTrackerComponent>(entity);
                        }
                    }

                    _stationWareChallenge.StartChallenge(_prototype.Index<ChallengePrototype>("TiebreakerChallenge"));
                }
                else
                {
                    GameTicker.EndRound();
                    StartPostRoundSlaughter(ev.Completions.Keys.ToList());
                    ware.RestartRoundTime = _timing.CurTime + ware.PostRoundDuration;
                    return;
                }

            }

            if (ware.ChallengeCount % ware.SpeedupInterval == 0)
            {
                ware.SpeedMultiplier -= ware.AmountPerSpeedup;
                _chatManager.DispatchServerAnnouncement(Loc.GetString("stationware-speed-up"), Color.Cyan);
                //todo: sfx
            }

            ware.ChallengeCount++;

            ware.CurrentChallenge = null;
            ware.NextChallengeTime = _timing.CurTime + ware.ChallengeDelay * ware.SpeedMultiplier;
        }
    }

    private bool CheckForTies([NotNullWhen(true)] out List<NetUserId>? players)
    {
        players = null;
        if (!_point.TryGetTiedPlayers(null, out var tiedPlayers))
            return false;

        if (tiedPlayers.Count > 1)
        {
            players = tiedPlayers;
            return true;
        }

        return false;
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        var query = EntityQueryEnumerator<StationWareRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var ware, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                return;

            // only do it between challenges
            if (ware.CurrentChallenge != null)
                return;

            if (ev.NewMobState != MobState.Dead)
                return;

            if (!TryComp<ActorComponent>(ev.Target, out var actor))
                return;

            ware.QueuedRespawns.Add(actor.PlayerSession);
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var query = EntityQueryEnumerator<StationWareRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out _, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                return;

            ev.AddLine(_point.GetPointScoreBoard().ToMarkup());

            if (!_point.TryGetHighestScoringPlayer(null, out var pair))
                return;
            var info = pair.Value.Value;
            ev.AddLine(Loc.GetString("stationware-report-winner",
                ("name", info.Name),
                ("points", info.Points)));
        }
    }

    protected override void Started(EntityUid uid, StationWareRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _point.CreatePointManager(); //initialize it for the overlay
    }

    protected override void Ended(EntityUid uid, StationWareRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        _overlay.BroadcastText("", false, Color.Green);
    }

    protected override void ActiveTick(EntityUid uid, StationWareRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.RestartRoundTime != null && _timing.CurTime > component.RestartRoundTime)
        {
            GameTicker.RestartRound();
            return;
        }

        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;

        _stationWareChallenge.RespawnPlayers(component.QueuedRespawns);
        component.QueuedRespawns.Clear();

        if (component.CurrentChallenge != null)
            return;

        if (_timing.CurTime < component.NextChallengeTime)
            return;

        var challenge = GetRandomChallenge(component, component.ChallengeCount == component.TotalChallenges);
        component.CurrentChallenge = _stationWareChallenge.StartChallenge(challenge, component.SpeedMultiplier);
        component.PreviousChallenges.Add(challenge.ID);
    }

    private ChallengePrototype GetRandomChallenge(StationWareRuleComponent component, bool bossRound, int? triedAmount = 0)
    {
        if (triedAmount >= 5)
        {
            // uhhhhhhhhhhh
            GameTicker.EndRound();
            return _prototype.EnumeratePrototypes<ChallengePrototype>().ToList()[0];
        }

        var available = _prototype.EnumeratePrototypes<ChallengePrototype>()
            .Where(p => !component.PreviousChallenges.Contains(p.ID))
            .Where(p => p.Tags.Contains("BossRound") == bossRound)
            .Where(p => !p.Tags.Contains("ChallengePreventPick"))
            .ToList();

        if (!available.Any())
        {
            component.PreviousChallenges.Clear();
            return GetRandomChallenge(component, bossRound, triedAmount + 1);
        }
        return _random.Pick(available);
    }

    private void StartPostRoundSlaughter(List<NetUserId> ids)
    {
        Dictionary<NetUserId, (IPlayerSession, EntityUid)> players = new();
        foreach (var id in ids)
        {
            if (_player.TryGetSessionById(id, out var session) && session.AttachedEntity is { } attachedEntity)
                players.Add(id, (session, attachedEntity));
        }

        if (!_point.TryGetHighestScoringPlayer(null, out var highest))
            return;
        var firstNetUserId = highest.Value.Key;
        if (players.TryGetValue(firstNetUserId, out var s))
        {
            var playerEnt = s.Item2;

            // notify players
            _chatManager.DispatchServerMessage(s.Item1, Loc.GetString("stationware-you-won"));
            _overlay.BroadcastText(Loc.GetString("overlay-player-won",
                        ("player", MetaData(playerEnt).EntityName)
                    ), true, Color.Yellow);
            _overlay.BroadcastText(Loc.GetString("stationware-you-won"), true, Color.Green, s.Item1);

            // actual slaughter
            var minigun = Spawn("WeaponMinigun", Transform(playerEnt).Coordinates); // gamerules suck dick anyways idgaf
            EnsureComp<UnremoveableComponent>(minigun); // no stealing allowed
            _hands.TryPickup(playerEnt, minigun, checkActionBlocker: false);
            _godmode.EnableGodmode(playerEnt);
        }

        players.Remove(firstNetUserId);
        foreach (var (_, ent) in players.Values)
        {
            if (!TryComp<CombatModeComponent>(ent, out var combatMode))
                continue;
            _combatMode.SetInCombatMode(ent, false, combatMode);
            RemComp(ent, combatMode);
        }
    }
}
