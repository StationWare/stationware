using Content.Shared.Movement.Systems;

namespace Content.Server._StationWare.WareEvents.Components;

[RegisterComponent]
public sealed class MoveSpeedWareEventComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Modifier = 1f;
}

public sealed class MoveSpeedWareEventSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MoveSpeedWareEventComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid, MoveSpeedWareEventComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.Modifier, component.Modifier);
    }
}
