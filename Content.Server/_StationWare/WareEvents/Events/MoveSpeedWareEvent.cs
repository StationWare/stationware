using Content.Server._StationWare.WareEvents.Components;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class MoveSpeedWareEvent : WareEvent
{
    /// <summary>
    /// Whether or not the effects of the event should
    /// be reset. Must be
    /// </summary>
    [DataField("permanent")]
    public bool Permanent = true;

    /// <summary>
    /// The minimum value for the change in movement speed.
    /// </summary>
    [DataField("minMoveSpeedModifier")]
    public float MinMoveSpeedModifier = 1;

    /// <summary>
    /// The maximum value for the change in movement speed.
    /// </summary>
    [DataField("maxMoveSpeedModifier")]
    public float MaxMoveSpeedModifier = 1;

    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var movementSpeedSys = entity.System<MovementSpeedModifierSystem>();
        foreach (var player in players)
        {
            var value = random.NextFloat(MinMoveSpeedModifier, MaxMoveSpeedModifier);
            entity.EnsureComponent<MoveSpeedWareEventComponent>(player).Modifier *= value;
            movementSpeedSys.RefreshMovementSpeedModifiers(player);
        }
    }

    public override void End(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        // this technically doesn't work if
        // it is followed by a non-permanent modifier but eh
        if (Permanent)
            return;

        var movementSpeedSys = entity.System<MovementSpeedModifierSystem>();
        foreach (var player in players)
        {
            entity.RemoveComponent<MoveSpeedWareEventComponent>(player);
            movementSpeedSys.RefreshMovementSpeedModifiers(player);
        }
    }
}
