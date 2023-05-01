using Robust.Server.Player;

namespace Content.Server._StationWare.GameRules;

[RegisterComponent]
public sealed class StationWareRuleComponent : Component
{
    //todo you lazy fuck go make this proper
    public TimeSpan NextChallengeTime;
    public TimeSpan? RestartRoundTime;
    public TimeSpan ChallengeDelay = TimeSpan.FromSeconds(5);
    public readonly TimeSpan PostRoundDuration = TimeSpan.FromSeconds(10);
    public EntityUid? CurrentChallenge;

    public readonly HashSet<string> PreviousChallenges = new();

    public readonly int TotalChallenges = 15;
    public int ChallengeCount = 1;
    public readonly int SpeedupInterval = 5;
    public readonly float AmountPerSpeedup = 0.15f;

    public float SpeedMultiplier = 1f;

    public readonly HashSet<IPlayerSession> QueuedRespawns = new();
}
