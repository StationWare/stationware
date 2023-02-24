using Robust.Shared.GameStates;

namespace Content.Shared._StationWare.Weapons.Melee;

/// <summary>
/// This is used for a weapon that
/// knocks players back when they are hit.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class KnockbackWeaponComponent : Component
{
    /// <summary>
    /// The strength of the knockback on hit
    /// </summary>
    [DataField("strength"), ViewVariables(VVAccess.ReadWrite)]
    public float Strength = 10;

    /// <summary>
    /// How far the knockback tries to throw you.
    /// </summary>
    [DataField("distance"), ViewVariables(VVAccess.ReadWrite)]
    public float Distance = 3;
}
