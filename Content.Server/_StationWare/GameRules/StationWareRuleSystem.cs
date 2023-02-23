using System.Linq;
using Content.Server._StationWare.Challenges;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.GameRules;

public sealed class StationWareRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    private TimeSpan _nextChallengeTime;
    private TimeSpan _challengeDelay = TimeSpan.FromSeconds(5);
    private EntityUid? _currentChallenge;

    private int _totalChallenges = 10;
    private int _challengeCount = 1;

    private readonly Dictionary<IPlayerSession, int> _points = new();

    public override string Prototype => "StationWare";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        _configuration.OnValueChanged(CCVars.StationWareTotalChallenges, e => _totalChallenges = e, true);
        _configuration.OnValueChanged(CCVars.StationWareChallengeCooldownLength, e => _challengeDelay = TimeSpan.FromSeconds(e), true);
    }

    private void OnChallengeEnd(ref ChallengeEndEvent ev)
    {
        if (ev.Challenge != _currentChallenge)
            return;

        foreach (var (session, won) in ev.Completions)
        {
            if (won)
                AdjustPlayerScore(session);
        }

        if (_challengeCount == _totalChallenges)
        {
            // TODO: make this slightly better
            // more actionable plan: add a 10 second end-screen delay
            // before booting everyone back into the lobby. (Deathmatch code)
            GameTicker.EndRound();
            GameTicker.RestartRound();
            return;
        }
        _challengeCount++;

        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (_points.Count == 0)
            return;

        var orderedList = _points.OrderByDescending(x => x.Value).ToList();

        for (var i = 0; i < orderedList.Count; i++)
        {
            var (session, points) = orderedList[i];
            ev.AddLine(Loc.GetString("stationware-report-score",
                ("place", i + 1),
                ("name", session.Name),
                ("points", points)));
        }

        var first = orderedList.First();
        ev.AddLine("");
        ev.AddLine(Loc.GetString("stationware-report-winner", ("name", first.Key.Name), ("points", first.Value)));
    }

    private void AdjustPlayerScore(IPlayerSession session)
    {
        if (!_points.ContainsKey(session))
            _points[session] = 0;
        _points[session]++;
    }

    public override void Started()
    {
        _challengeCount = 1;
        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    public override void Ended() { }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!RuleStarted)
            return;

        if (_currentChallenge != null)
            return;

        if (_timing.CurTime < _nextChallengeTime)
            return;

        var challenge = _random.Pick(_prototype.EnumeratePrototypes<ChallengePrototype>().ToList());
        _currentChallenge = _stationWareChallenge.StartChallenge(challenge);
    }
}
