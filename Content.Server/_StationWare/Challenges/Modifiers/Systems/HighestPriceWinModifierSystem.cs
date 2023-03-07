using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Cargo.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class HighestPriceWinModifierSystem : EntitySystem
{
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HighestPriceWinModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, HighestPriceWinModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        if (!args.Players.Any())
            return;
        var (maxPlayer, maxCost) = (args.Players.First(), 0.0);
        foreach (var ent in args.Players)
        {
            var price = _pricing.GetPrice(ent);
            if (price <= maxCost)
                continue;
            maxPlayer = ent;
            maxCost = price;
        }

        _stationWareChallenge.SetPlayerChallengeState(maxPlayer, uid, true, args.Component);
    }
}
