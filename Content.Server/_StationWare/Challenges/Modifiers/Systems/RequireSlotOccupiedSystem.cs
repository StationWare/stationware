using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Inventory;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class RequireSlotOccupiedSystem : EntitySystem
{
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;
    [Dependency] private readonly InventorySystem _invSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireSlotOccupiedComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, RequireSlotOccupiedComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            _invSystem.TryGetSlotEntity(player, component.Slot, out var slotEntity);

            // assume we need to check prototype
            if (slotEntity == null)
                continue;

            if (component.Whitelist == null)
            {
                _stationWareChallenge.SetPlayerChallengeState(player, uid, true, args.Component);
            }
            else
            {
                if (component.Whitelist.IsValid(slotEntity.Value))
                    _stationWareChallenge.SetPlayerChallengeState(player, uid, true, args.Component);
            }
        }
    }
}
