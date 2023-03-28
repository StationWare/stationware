using Content.Server.Chat.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

public sealed class SayAnythingModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SayAnythingModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SayAnythingPlayerComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
        SubscribeLocalEvent<SayAnythingPlayerComponent, EntitySpokeEvent>(OnTransformSpeech);
    }

    private void OnChallengeStart(EntityUid uid, SayAnythingModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<SayAnythingPlayerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeStateSet(EntityUid uid, SayAnythingPlayerComponent component, ref PlayerChallengeStateSetEvent args)
    {
        RemComp(uid, component);
    }

    private void OnTransformSpeech(EntityUid uid, SayAnythingPlayerComponent component, EntitySpokeEvent args)
    {
        if (!TryComp<SayAnythingModifierComponent>(component.Challenge, out var modifier))
            return;

        _stationWareChallenge.SetPlayerChallengeState(uid, component.Challenge, modifier.ShouldSpeak);
    }
}
