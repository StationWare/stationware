using Content.Shared.Storage;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class SpawnEntityModifierComponent : Component
{
    /// <summary>
    /// What entities will be spawned at the bus entrance
    /// </summary>
    [DataField("spawns", required: true)]
    public List<EntitySpawnEntry> Spawns = default!;

    /// <summary>
    /// Should the entities be spawned in clumps.
    /// If so, how large?
    /// </summary>
    [DataField("clumpSize")]
    public int? ClumpSize;

    /// <summary>
    /// A scalar applied to the grid when selecting spawning locations.
    /// </summary>
    [DataField("spawnLocationScalar")]
    public float SpawnLocationScalar = 1f;

    [DataField("spawnedEntities"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<EntityUid> SpawnedEntities = new();
}
