using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Buckle.Systems;
using Content.Shared.Buckle.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class BuckledWinModifierSystem : EntitySystem
{
    [Dependency] private readonly BuckleSystem _buckle = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BuckledWinModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, BuckledWinModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            if (TryComp<BuckleComponent>(player, out var buckle) && buckle.Buckled)
            {
                _buckle.TryUnbuckle(player, player, true, buckle);
                _stationWareChallenge.SetPlayerChallengeState(player, uid, true, args.Component);
            }
        }
    }
}
