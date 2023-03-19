using System.Text.RegularExpressions;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Chat.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class SayPhraseModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SayPhraseModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SayPhrasePlayerComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
        SubscribeLocalEvent<SayPhrasePlayerComponent, EntitySpokeEvent>(OnTransformSpeech);
    }

    private void OnChallengeStart(EntityUid uid, SayPhraseModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<SayPhrasePlayerComponent>(player).Challenge = uid;
        }
    }

    private void OnChallengeStateSet(EntityUid uid, SayPhrasePlayerComponent component, ref PlayerChallengeStateSetEvent args)
    {
        RemComp(uid, component);
    }

    private void OnTransformSpeech(EntityUid uid, SayPhrasePlayerComponent component, EntitySpokeEvent args)
    {
        if (!TryComp<SayPhraseModifierComponent>(component.Challenge, out var modifier))
            return;

        var modifiedSpeech = args.Message.ToLower();
        var target = modifier.Phrase.ToLower();
        var contains = Regex.IsMatch(modifiedSpeech, "(?=.*("+target+"))");
        if (!contains && !modifier.WrongPhraseFail)
            return;
        _stationWareChallenge.SetPlayerChallengeState(uid, component.Challenge, contains);
    }
}
