﻿using System.Linq;
using Content.Server._StationWare.ChallengeOverlay;
using Content.Server._StationWare.Challenges;
using Content.Server.Chat.Managers;
using Content.Server.CombatMode;
using Content.Server.Damage.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Hands.Systems;
using Content.Shared.CCVar;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.GameRules;

public sealed class StationWareRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GodmodeSystem _godmode = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private readonly ChallengeOverlaySystem _overlay = default!;

    private TimeSpan _nextChallengeTime;
    private TimeSpan? _restartRoundTime;
    private TimeSpan _challengeDelay = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _postRoundDuration = TimeSpan.FromSeconds(10);
    private EntityUid? _currentChallenge;

    private readonly HashSet<string> _previousChallenges = new();

    private int _totalChallenges = 10;
    private int _challengeCount = 1;

    private readonly HashSet<IPlayerSession> _queuedRespawns = new();

    public override string Prototype => "StationWare";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        _overlay.BroadcastText("", false, Color.Green, null);


        _configuration.OnValueChanged(CCVars.StationWareTotalChallenges, e => _totalChallenges = e, true);
        _configuration.OnValueChanged(CCVars.StationWareChallengeCooldownLength, e => _challengeDelay = TimeSpan.FromSeconds(e), true);
    }

    private void OnChallengeEnd(ref ChallengeEndEvent ev)
    {
        if (!RuleStarted)
            return;

        if (ev.Challenge != _currentChallenge)
            return;

        if (_challengeCount == _totalChallenges)
        {
            GameTicker.EndRound();
            StartPostRoundSlaughter(ev.Completions.Keys.ToList());
            _restartRoundTime = _timing.CurTime + _postRoundDuration;
            return;
        }

        _challengeCount++;

        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (!RuleStarted)
            return;

        // only do it between challenges
        if (_currentChallenge != null)
            return;

        if (ev.NewMobState != MobState.Dead)
            return;

        if (!TryComp<ActorComponent>(ev.Target, out var actor))
            return;

        if (!_queuedRespawns.Contains(actor.PlayerSession))
            _queuedRespawns.Add(actor.PlayerSession);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        if (_points.Count == 0)
            return;

        var orderedList = _points.OrderByDescending(x => x.Value.Points).ToList();

        for (var i = 0; i < orderedList.Count; i++)
        {
            var (_, playerInfo) = orderedList[i];
            ev.AddLine(Loc.GetString("stationware-report-score",
                ("place", i + 1),
                ("name", playerInfo.Name),
                ("points", playerInfo.Points)));
        }

        var (_, fPlayerInfo) = orderedList.First();
        ev.AddLine("");
        ev.AddLine(Loc.GetString("stationware-report-winner",
            ("name", fPlayerInfo.Name),
            ("points", fPlayerInfo.Points)));
    }

    public override void Started()
    {
        _previousChallenges.Clear();
        _challengeCount = 1;
        _currentChallenge = null;
        _restartRoundTime = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    public override void Ended() { }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!RuleStarted)
            return;

        if (_restartRoundTime != null && _timing.CurTime > _restartRoundTime)
        {
            GameTicker.RestartRound();
            return;
        }

        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;

        foreach (var queued in _queuedRespawns)
        {
            _stationWareChallenge.RespawnPlayer(queued);
        }
        _queuedRespawns.Clear();

        if (_currentChallenge != null)
            return;

        if (_timing.CurTime < _nextChallengeTime)
            return;

        var challenge = GetRandomChallenge();
        _currentChallenge = _stationWareChallenge.StartChallenge(challenge);
        _previousChallenges.Add(challenge.ID);
    }

    private ChallengePrototype GetRandomChallenge()
    {
        var available = _prototype.EnumeratePrototypes<ChallengePrototype>()
            .Where(p => !_previousChallenges.Contains(p.ID))
            .ToList();
        if (!available.Any())
        {
            _previousChallenges.Clear();
            return GetRandomChallenge();
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

        var firstNetUserId = _points.MaxBy(x => x.Value.Points).Key;
        if (players.TryGetValue(firstNetUserId, out var s))
        {
            var playerEnt = s.Item2;
            _godmode.EnableGodmode(playerEnt);
            _chatManager.DispatchServerMessage(s.Item1, Loc.GetString("stationware-you-won"));
            _overlay.BroadcastText(Loc.GetString("stationware-you-won"), true, Color.Green, s.Item1);
            var minigun = Spawn("WeaponMinigun", Transform(playerEnt).Coordinates); // gamerules suck dick anyways idgaf
            EnsureComp<UnremoveableComponent>(minigun); // no stealing allowed
            _hands.TryPickup(playerEnt, minigun, checkActionBlocker: false);
        }

        players.Remove(firstNetUserId);
        foreach (var (_, ent) in players.Values)
        {
            if (!TryComp<CombatModeComponent>(ent, out var combatMode))
                continue;
            combatMode.IsInCombatMode = false;
            RemComp(ent, combatMode);
        }
    }
}
