using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class WinLimiterSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WinLimiterComponent, PlayerChallengeStateSetEvent>(OnPlayerChallengeStateSet);
    }

    private void OnPlayerChallengeStateSet(EntityUid uid, WinLimiterComponent component, ref PlayerChallengeStateSetEvent args)
    {
        if (component.Ending || !args.Won)
            return;

        var totalPlayers = args.Component.Completions.Count;

        var limit = component.LimitPercent == null
            ? component.Limit
            : (int) Math.Clamp(totalPlayers * component.LimitPercent.Value, 0, args.Component.Completions.Count);

        var wins = args.Component.Completions.Count(p => p.Value == true);
        if (wins < limit)
            return;
        component.Ending = true;
        foreach (var player in args.Component.Completions.Keys)
        {
            _stationWareChallenge.SetPlayerChallengeState(player, uid, false, args.Component);
        }
    }
}
