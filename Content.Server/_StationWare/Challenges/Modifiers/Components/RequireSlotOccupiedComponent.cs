using Content.Shared.Whitelist;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for events to check whether their slot has a prototype or is generally occupied
/// </summary>
[RegisterComponent]
public sealed class RequireSlotOccupiedComponent : Component
{
    [DataField("slot")] public string Slot = "head";
    [DataField("requiredPrototype")] public EntityWhitelist? RequiredPrototype = null;
}

/// <summary>
/// Tracking players for <see cref="RequireSlotOccupiedComponent"/>
/// </summary>
[RegisterComponent]
public sealed class RequireSlotOccupiedTrackerComponent : Component
{
    /// <summary>
    /// The challenge entity for the slot occupation modifier player
    /// </summary>
    [DataField("challenge")]
    public EntityUid Challenge;
}
