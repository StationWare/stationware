using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class StayAliveModifierSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StayAliveModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, StayAliveModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            _stationWareChallenge.SetPlayerChallengeState(player, uid, _mobState.IsAlive(player));
        }
    }
}
