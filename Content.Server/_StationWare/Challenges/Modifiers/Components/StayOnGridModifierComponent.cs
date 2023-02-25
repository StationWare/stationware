namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for ensuring that players stay on the grid
/// to win the challenge. There's technically ways to cheese this but idgaf
/// </summary>
[RegisterComponent]
public sealed class StayOnGridModifierComponent : Component
{
}

[RegisterComponent]
public sealed class StayOnGridTrackerComponent : Component
{
    /// <summary>
    /// The challenge this tracker is registered to.
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;

    /// <summary>
    /// Did they win or lose
    /// </summary>
    [DataField("lost")]
    public bool Lost = false;
}
