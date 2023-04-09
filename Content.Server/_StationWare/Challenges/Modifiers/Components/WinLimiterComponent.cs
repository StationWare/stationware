namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for limiting the amount of winners in a challenge.
/// </summary>
[RegisterComponent]
public sealed class WinLimiterComponent : Component
{
    /// <summary>
    /// The numerical amount of players that are allowed to win.
    /// </summary>
    [DataField("limit")]
    public int Limit;

    /// <summary>
    /// A percentag of players that are allowed to win. Overrides <see cref="Limit"/>
    /// </summary>
    [DataField("limitPercent")]
    public float? LimitPercent;

    public bool Ending;
}
