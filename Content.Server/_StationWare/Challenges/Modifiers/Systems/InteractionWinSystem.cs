using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Interaction;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class InteractionWinSystem : EntitySystem
{
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
            _stationWareChallenge.SetPlayerChallengeState(player, challengeEnt, component.WinOnInteract, challenge);
        }
    }
}
