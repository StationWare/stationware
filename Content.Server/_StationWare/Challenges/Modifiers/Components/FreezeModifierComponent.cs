namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// Players musn't move or else they lose.
/// </summary>
[RegisterComponent]
public sealed class FreezeModifierComponent : Component
{
}

/// <summary>
/// A player that is playing <see cref="FreezeModifierComponent"/>
/// </summary>
[RegisterComponent]
public sealed class FreezePlayerComponent : Component
{
    /// <summary>
    /// The challenge entity for the freeze modifier player
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;
}
