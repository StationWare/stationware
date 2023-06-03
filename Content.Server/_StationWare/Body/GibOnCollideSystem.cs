using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Events;

namespace Content.Server._StationWare.Body;

public sealed class GibOnCollideSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GibOnCollideComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, GibOnCollideComponent component, ref StartCollideEvent args)
    {
        var otherEnt = args.OtherEntity;
        if (!HasComp<MobStateComponent>(otherEnt) || !TryComp<BodyComponent>(otherEnt, out var body))
            return;
        _body.GibBody(otherEnt, true, body, true);
        if (!component.AllowMultipleHits)
            Del(uid);
    }
}
