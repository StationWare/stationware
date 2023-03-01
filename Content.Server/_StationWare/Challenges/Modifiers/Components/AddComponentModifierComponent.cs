using Robust.Shared.Prototypes;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

public sealed class AddComponentModifierComponent : Component
{
    /// <summary>
    /// The components that are added :)
    /// </summary>
    [DataField("addedComponents")] public EntityPrototype.ComponentRegistry AddedComponents = new();

    [DataField("providedComponents"), ViewVariables(VVAccess.ReadWrite)]
    public readonly HashSet<Component> ProvidedComponents = new();
}
