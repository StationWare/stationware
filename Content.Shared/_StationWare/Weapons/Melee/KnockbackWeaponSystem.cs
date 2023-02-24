using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._StationWare.Weapons.Melee;

public sealed class KnockbackWeaponSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<KnockbackWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, KnockbackWeaponComponent component, MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var userXForm = Transform(args.User);
        foreach (var hit in args.HitEntities)
        {
            var hitXForm = Transform(hit);
            var direction = hitXForm.MapPosition.Position - userXForm.MapPosition.Position;
            direction = direction.Normalized * component.Distance;
            if (direction == Vector2.NaN)
                continue;
            _throwing.TryThrow(hit, direction, component.Strength, args.User, transform: hitXForm);
        }
    }
}
