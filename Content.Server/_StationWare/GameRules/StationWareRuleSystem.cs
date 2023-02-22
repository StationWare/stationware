using System.Linq;
using Content.Server._StationWare.Challenges;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.GameRules;

public sealed class StationWareRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    private TimeSpan _nextChallengeTime;
    private readonly TimeSpan _challengeDelay = TimeSpan.FromSeconds(5); //CVAR
    private EntityUid? _currentChallenge;

    private int _totalRounds = 2; //CVAR
    private int _currentRound = 1;

    private readonly Dictionary<IPlayerSession, int> _points = new();

    public override string Prototype => "StationWare";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
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

        if (_currentRound == _totalRounds)
        {
            // TODO: make this slightly better
            GameTicker.EndRound();
            GameTicker.RestartRound();
            return;
        }
        _currentRound++;

        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (_points.Count == 0)
            return;

        foreach (var (session, points) in _points)
        {
            //TODO: loc, don't disappoint ruskis
            ev.AddLine($"{session.Name} got {points} points.");
        }

        var first = _points.First();
        ev.AddLine("");
        ev.AddLine($"{first.Key.Name} was the winner with a total of {first.Value} points.");
    }

    private void AdjustPlayerScore(IPlayerSession session)
    {
        if (!_points.ContainsKey(session))
            _points[session] = 0;
        _points[session]++;
    }

    public override void Started()
    {
        _currentRound = 1;
        _currentChallenge = null;
        _nextChallengeTime = _timing.CurTime + _challengeDelay;
    }

    public override void Ended() { }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!RuleStarted)
            return;

        // TODO: 5 second delay between challenges
        if (_currentChallenge != null)
            return;

        if (_timing.CurTime < _nextChallengeTime)
            return;

        var challenge = _random.Pick(_prototype.EnumeratePrototypes<ChallengePrototype>().ToList());
        _currentChallenge = _stationWareChallenge.StartChallenge(challenge);
    }
}
