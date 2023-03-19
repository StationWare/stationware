namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class KeepMovingModifierComponent : Component
{

}

/// <summary>
/// A player that is playing <see cref="KeepMovingModifierComponent"/>
/// </summary>
[RegisterComponent]
public sealed class KeepMovingPlayerComponent : Component
{
    /// <summary>
    /// The challenge entity for the freeze modifier player
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;
}
