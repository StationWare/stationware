using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Interaction;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class InteractionWinSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionWinComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(EntityUid uid, InteractionWinComponent component, InteractHandEvent args)
    {
        var player = args.User;
        foreach (var challenge in EntityQuery<StationWareChallengeComponent>())
        {
            var challengeEnt = challenge.Owner;
            if (!challenge.Participants.ContainsValue(player))
                continue;
            var won = _random.Prob(component.WinChance);
            if (!won && !component.FailOnNoWin)
                continue;
            _stationWareChallenge.SetPlayerChallengeState(player, challengeEnt, won, challenge);
        }
    }
}
