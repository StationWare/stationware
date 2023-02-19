using Content.Server._StationWare.WareEvents.Events;

namespace Content.Server._StationWare.WareEvents.Components;

/// <summary>
/// Tracks if a player has had their hands removed via a ware event.
/// </summary>
[RegisterComponent]
public sealed class AddHandWareEventComponent : Component
{
    /// <summary>
    /// The net change in hands via <see cref="AddHandWareEvent"/>
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Change;
}
