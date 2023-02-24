using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Movement.Events;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class FreezeModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<FreezeModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<FreezeModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<FreezePlayerComponent, MoveInputEvent>(OnMoveInput);
    }

    private void OnChallengeStart(EntityUid uid, FreezeModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<FreezePlayerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeEnd(EntityUid uid, FreezeModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var freezePlayer in EntityQuery<FreezePlayerComponent>())
        {
            if (freezePlayer.Challenge == uid)
                RemComp(freezePlayer.Owner, freezePlayer);
        }
    }

    private void OnMoveInput(EntityUid uid, FreezePlayerComponent component, ref MoveInputEvent args)
    {
        _stationWareChallenge.SetPlayerChallengeState(uid, component.Challenge, false);
    }
}
