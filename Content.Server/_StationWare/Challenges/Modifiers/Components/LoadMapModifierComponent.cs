using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class LoadMapModifierComponent : Component
{
    /// <summary>
    /// Relative directory path to the given map, i.e. `Maps/ChallengeMaps/map-test.yml`
    /// </summary>
    [DataField("mapPath", required: true)]
    public ResPath MapPath { get; } = default!;

    /// <summary>
    /// The map created by the modifier
    /// </summary>
    public MapId? Map;
}

[RegisterComponent]
public sealed class MapPlayerSpawnerComponent : Component
{

}
