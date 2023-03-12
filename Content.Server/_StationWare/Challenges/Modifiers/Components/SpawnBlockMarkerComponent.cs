using Content.Server._StationWare.Challenges.Modifiers.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
///     Prevents <see cref="SpawnEntityModifierSystem"/> from spawning entities on areas mapped with this marker.
///     This is for 'out of play' areas which are nonetheless visible, such as on Magma Factory.
/// </summary>
/// <remarks>
///     This only works on anchored entities, since tile-bound.
/// </remarks>
[RegisterComponent]
public sealed class SpawnBlockMarkerComponent : Component
{
}
