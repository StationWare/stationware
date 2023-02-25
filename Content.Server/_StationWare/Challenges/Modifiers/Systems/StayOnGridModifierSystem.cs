using Content.Server._StationWare.Challenges.Modifiers.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class StayOnGridModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StayOnGridModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<StayOnGridModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<StayOnGridTrackerComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnChallengeStart(EntityUid uid, StayOnGridModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<StayOnGridTrackerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeEnd(EntityUid uid, StayOnGridModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var tracker in EntityQuery<StayOnGridTrackerComponent>())
        {
            if (tracker.Challenge == uid)
                RemComp(tracker.Owner, tracker);
        }
    }

    private void OnParentChanged(EntityUid uid, StayOnGridTrackerComponent component, ref EntParentChangedMessage args)
    {
        if (args.Transform.GridUid == null)
            component.Lost = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var tracker in EntityQuery<StayOnGridTrackerComponent>())
        {
            var uid = tracker.Owner;
            if (!tracker.Lost)
                return;
            _stationWareChallenge.SetPlayerChallengeState(uid, tracker.Challenge, false);
            RemComp(uid, tracker);
        }
    }
}
