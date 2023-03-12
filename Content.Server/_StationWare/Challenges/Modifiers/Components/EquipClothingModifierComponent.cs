using Content.Shared.Storage;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for equiping players
/// with clothing during a challenge.
/// </summary>
[RegisterComponent]
public sealed class EquipClothingModifierComponent : Component
{
    /// <summary>
    /// What entities will be spawned at the bus entrance
    /// </summary>
    [DataField("spawns", required: true)]
    public List<EntitySpawnEntry> Spawns = default!;

    /// <summary>
    /// Tracks what we've actually spawned
    /// </summary>
    [DataField("spawnedEntities"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<EntityUid> SpawnedEntities = new();

    /// <summary>
    /// What percentage of players are receiving the clothing?
    /// </summary>
    [DataField("receivingPercentage")]
    public float ReceivingPercentage = 1f;
}
