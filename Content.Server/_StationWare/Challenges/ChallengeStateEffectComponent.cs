namespace Content.Server._StationWare.Challenges;

/// <summary>
/// This is used for tracking the visual effects that
/// mark successes and failures at a particular challenge.
/// </summary>
[RegisterComponent]
public sealed class ChallengeStateEffectComponent : Component
{
    /// <summary>
    /// The challenge that was won/lost
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;

    /// <summary>
    /// How long does the effect persist after the end of
    /// a challenge?
    /// </summary>
    [DataField("disappearDelay")]
    public float DisappearDelay = 2.5f;
}
