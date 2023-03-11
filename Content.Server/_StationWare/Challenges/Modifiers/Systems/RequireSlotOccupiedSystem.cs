using System.Linq;
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
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireSlotOccupiedComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<RequireSlotOccupiedComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, RequireSlotOccupiedComponent component,
        ref ChallengeStartEvent args)
    {
        foreach (var player in args.Players)
        {
            var winning = _componentFactory.GetComponent<RequireSlotOccupiedTrackerComponent>();
            winning.Challenge = uid;
            winning.Owner = player;
            EntityManager.AddComponent(player, winning);
        }
    }

    private void OnBeforeChallengeEnd(EntityUid uid, RequireSlotOccupiedComponent component, ref BeforeChallengeEndEvent args)
    {
        // hi emo

        foreach (var tracker in EntityQuery<RequireSlotOccupiedTrackerComponent>())
        {
            var ent = tracker.Owner;

            _invSystem.TryGetSlotEntity(ent, component.Slot, out var slotEntity);


            if (component.RequiredPrototype == null)
            {
                _stationWareChallenge.SetPlayerChallengeState(ent, uid, true, args.Component);
            }
            else
            {
                // assume we need to check prototype
                if (slotEntity == null)
                    continue;

                if (component.RequiredPrototype.IsValid(slotEntity.Value))
                   _stationWareChallenge.SetPlayerChallengeState(ent, uid, true, args.Component);
            }

            RemComp(ent, tracker);
        }
    }
}
