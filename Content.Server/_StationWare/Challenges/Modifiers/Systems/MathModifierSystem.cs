using System.Globalization;
using Content.Server._StationWare.ChallengeOverlay;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class MathModifierSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChallengeOverlaySystem _overlay = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MathModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<MathPlayerComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
        SubscribeLocalEvent<MathPlayerComponent, EntitySpokeEvent>(OnSpoke);
    }

    private void OnChallengeStart(EntityUid uid, MathModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            EnsureComp<MathPlayerComponent>(player).Challenge = uid;
        }

        var leftHand = _random.Next(component.MinimumRange, component.MaximumRange);
        var rightHand = _random.Next(component.MinimumRange, component.MaximumRange);
        var operand = _random.Next(0, 3);
        var equation = "";
        var answer = 0;

        switch (operand)
        {
            case 0:
                equation = leftHand + " + " + rightHand;
                answer = leftHand + rightHand;
                break;
            case 1:
                equation = leftHand + " - " + rightHand;
                answer = leftHand - rightHand;
                break;
            case 2:
                equation = leftHand + " * " + rightHand;
                answer = leftHand * rightHand;
                break;
        }

        component.Answer = (int) MathF.Round(answer);
        _overlay.BroadcastText(Loc.GetString("challenge-math-equation", ("equation", equation)), true, Color.Fuchsia);
    }

    private void OnChallengeStateSet(EntityUid uid, MathPlayerComponent component, ref PlayerChallengeStateSetEvent args)
    {
        RemComp(uid, component);
    }

    private void OnSpoke(EntityUid uid, MathPlayerComponent component, EntitySpokeEvent args)
    {
        if (!TryComp<MathModifierComponent>(component.Challenge, out var modifier))
            return;

        if (args.Message != modifier.Answer.ToString(CultureInfo.InvariantCulture))
            return;

        _stationWareChallenge.SetPlayerChallengeState(uid, component.Challenge, true);
    }
}
