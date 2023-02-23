using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class WinningMarkerModifierComponent : Component
{
    /// <summary>
    /// The amount of the markers to give out.
    /// </summary>
    [DataField("amount")]
    public int Amount = 1;

    /// <summary>
    /// The prototype ID of the effect that will
    /// display above the player.
    /// </summary>
    [DataField("markerEffectPrototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? MarkerEffectPrototype;
}
