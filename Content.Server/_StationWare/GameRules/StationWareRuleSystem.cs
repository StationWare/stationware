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
using Content.Server.Hands.Systems;
using Content.Shared.CCVar;
using Content.Shared.CombatMode;
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
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    [Dependency] private readonly GodmodeSystem _godmode = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private readonly ChallengeOverlaySystem _overlay = default!;
    [Dependency] private readonly PointSystem _point = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private TimeSpan _nextChallengeTime;
    private TimeSpan? _restartRoundTime;
    private TimeSpan _challengeDelay = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _postRoundDuration = TimeSpan.FromSeconds(10);
    private EntityUid? _currentChallenge;

    private readonly HashSet<string> _previousChallenges = new();

    private int _totalChallenges = 15;
    private int _challengeCount = 1;
    private int _speedupInterval = 5;
    private float _amountPerSpeedup = 0.15f;

    private float _speedMultiplier = 1f;

    private readonly HashSet<IPlayerSession> _queuedRespawns = new();

    public override string Prototype => "StationWare";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        _overlay.BroadcastText("", false, Color.Green);

        _configuration.OnValueChanged(CCVars.StationWareTotalChallenges, e => _totalChallenges = e, true);
        _configuration.OnValueChanged(CCVars.StationWareChallengeCooldownLength, e => _challengeDelay = TimeSpan.FromSeconds(e), true);
        _configuration.OnValueChanged(CCVars.StationWareSpeedupInterval, e => _speedupInterval = e, true);
        _configuration.OnValueChanged(CCVars.StationWareAmountPerSpeedup, e => _amountPerSpeedup = e, true);
    }

    private void OnChallengeEnd(ref ChallengeEndEvent ev)
    {
        if (!RuleStarted)
            return;

        if (ev.Challenge != _currentChallenge)
            return;

        if (_challengeCount >= _totalChallenges)
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
                _restartRoundTime = _timing.CurTime + _postRoundDuration;
                return;
            }
        }

        if (_challengeCount % _speedupInterval == 0)
        {
            _speedMultiplier -= _amountPerSpeedup;
            _chatManager.DispatchServerAnnouncement(Loc.GetString("stationware-speed-up"), Color.Cyan);
            //todo: sfx
        }
        _challengeCount++;

        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay * _speedMultiplier;
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
        if (!RuleStarted)
            return;

        // only do it between challenges
        if (_currentChallenge != null)
            return;

        if (ev.NewMobState != MobState.Dead)
            return;

        if (!TryComp<ActorComponent>(ev.Target, out var actor))
            return;

        _queuedRespawns.Add(actor.PlayerSession);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (!RuleAdded)
            return;

        ev.AddLine(_point.GetPointScoreBoard().ToMarkup());

        if (!_point.TryGetHighestScoringPlayer(null, out var pair))
            return;
        var info = pair.Value.Value;
        ev.AddLine(Loc.GetString("stationware-report-winner",
            ("name", info.Name),
            ("points", info.Points)));
    }

    public override void Started()
    {
        _previousChallenges.Clear();
        _challengeCount = 1;
        _currentChallenge = null;
        _restartRoundTime = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
        _speedMultiplier = 1;
        _point.CreatePointManager(); //initialize it for the overlay
    }

    public override void Ended()
    {
        _overlay.BroadcastText("", false, Color.Green);
    }

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

        _stationWareChallenge.RespawnPlayers(_queuedRespawns);
        _queuedRespawns.Clear();

        if (_currentChallenge != null)
            return;

        if (_timing.CurTime < _nextChallengeTime)
            return;

        var challenge = GetRandomChallenge(_challengeCount == _totalChallenges);
        _currentChallenge = _stationWareChallenge.StartChallenge(challenge, _speedMultiplier);
        _previousChallenges.Add(challenge.ID);
    }

    private ChallengePrototype GetRandomChallenge(bool bossRound, int? triedAmount = 0)
    {
        if (triedAmount >= 5)
        {
            // uhhhhhhhhhhh
            GameTicker.EndRound();
            return _prototype.EnumeratePrototypes<ChallengePrototype>().ToList()[0];
        }

        var available = _prototype.EnumeratePrototypes<ChallengePrototype>()
            .Where(p => !_previousChallenges.Contains(p.ID))
            .Where(p => p.Tags.Contains("BossRound") == bossRound)
            .Where(p => !p.Tags.Contains("ChallengePreventPick"))
            .ToList();

        if (!available.Any())
        {
            _previousChallenges.Clear();
            return GetRandomChallenge(bossRound, triedAmount + 1);
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
