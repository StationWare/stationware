namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for marking a player as the winner of a challenge.
/// Hitting them causes the crown to transfer to that player.
/// </summary>
[RegisterComponent]
public sealed class WinningMarkerComponent : Component
{
    /// <summary>
    /// The challenge that this winner belongs to
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;

    /// <summary>
    /// The effect marker
    /// </summary>
    [DataField("marker")]
    public EntityUid? Marker;
}
