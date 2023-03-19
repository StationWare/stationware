using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class KeepMovingModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<KeepMovingModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<KeepMovingPlayerComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
    }

    private void OnChallengeStart(EntityUid uid, KeepMovingModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<KeepMovingPlayerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeStateSet(EntityUid uid, KeepMovingPlayerComponent component, ref PlayerChallengeStateSetEvent args)
    {
        RemComp(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<KeepMovingPlayerComponent, PhysicsComponent>();
        while (enumerator.MoveNext(out var uid, out var keepMoving, out var physics))
        {
            if (physics.LinearVelocity != Vector2.Zero)
                continue;
            _stationWareChallenge.SetPlayerChallengeState(uid, keepMoving.Challenge, false);
        }
    }
}
