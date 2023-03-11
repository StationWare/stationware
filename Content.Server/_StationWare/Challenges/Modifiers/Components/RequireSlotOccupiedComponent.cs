using Content.Shared.Whitelist;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for events to check whether their slot has a prototype or is generally occupied
/// </summary>
[RegisterComponent]
public sealed class RequireSlotOccupiedComponent : Component
{
    /// <summary>
    /// The slot that must be occupied
    /// </summary>
    [DataField("slot", required: true)]
    public string Slot = "head";

    /// <summary>
    /// A whitelist for what the entity occupying the slot must be in order to win.
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;
}

