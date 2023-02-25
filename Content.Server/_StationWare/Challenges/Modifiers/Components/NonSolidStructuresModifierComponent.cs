using Content.Shared.Tag;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for making all structures non-solid during a challenge.
/// </summary>
[RegisterComponent]
public sealed class NonSolidStructuresModifierComponent : Component
{
    [DataField("targetTag", customTypeSerializer: typeof(PrototypeIdSerializer<TagPrototype>))]
    public string TargetTag = "Structure";

    [DataField("affectedFixtures")]
    public HashSet<Fixture> AffectedFixtures = new();
}
