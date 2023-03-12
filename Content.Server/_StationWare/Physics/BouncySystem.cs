using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Physics;

public sealed class BouncySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BouncyComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, BouncyComponent component, MapInitEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        var fixtures = Comp<FixturesComponent>(uid);
        _physics.SetBodyStatus(physics, BodyStatus.InAir);
        _physics.WakeBody(uid, manager: fixtures, body: physics);

        foreach (var fixture in fixtures.Fixtures.Values)
        {
            if (!fixture.Hard)
                continue;

            _physics.SetRestitution(uid, fixture, 1f, false, fixtures);
        }
        _fixture.FixtureUpdate(uid, manager: fixtures, body: physics);

        _physics.SetLinearVelocity(uid, _random.NextVector2(component.MinLinearVelocity, component.MaxLinearVelocity), manager: fixtures, body: physics);
        _physics.SetAngularVelocity(uid, MathF.PI * component.AngularVelocity, manager: fixtures, body: physics);
    }
}
