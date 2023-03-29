using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Chat.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class EmoteModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EmoteModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<EmotePlayerComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
        SubscribeLocalEvent<EmotePlayerComponent, EmoteEvent>(OnEmote);
    }

    private void OnChallengeStart(EntityUid uid, EmoteModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<EmotePlayerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeStateSet(EntityUid uid, EmotePlayerComponent component, ref PlayerChallengeStateSetEvent args)
    {
        RemComp(uid, component);
    }

    private void OnEmote(EntityUid uid, EmotePlayerComponent component, ref EmoteEvent args)
    {
        if (!TryComp<EmoteModifierComponent>(component.Challenge, out var modifier))
            return;

        var correctEmote = modifier.TargetEmotes?.Contains(args.Emote.ID) ?? true;
        if (!correctEmote && !modifier.FailOnIncorrectEmote)
            return;

        if (!modifier.ShouldEmote)
            correctEmote = !correctEmote;

        _stationWareChallenge.SetPlayerChallengeState(uid, component.Challenge, correctEmote);
    }
}
