using Content.Shared.Storage;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class GiveItemModifierComponent : Component
{
    /// <summary>
    /// What entities will be spawned and given to the player.
    /// </summary>
    [DataField("items", required: true)]
    public List<EntitySpawnEntry> Items = default!;

    /// <summary>
    /// What amount of the player population will be receiving these items?
    /// </summary>
    [DataField("populationAmount")]
    public int? PopulationAmount;

    /// <summary>
    /// What percentage of the player population will be receiving these items?
    /// </summary>
    [DataField("populationPercentage")]
    public float? PopulationPercentage;

    [DataField("providedItems"), ViewVariables(VVAccess.ReadWrite)]
    public readonly HashSet<EntityUid> ProvidedItems = new();
}
