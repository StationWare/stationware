using System.Linq;
using Content.Server._StationWare.WareEvents.Components;
using Content.Server.Hands.Components;
using Content.Server.Hands.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Hands.Components;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class AddHandWareEvent : WareEvent
{
    /// <summary>
    /// A counter used to generate unique hand names.
    /// static so that it works between repeated calls of the event
    /// </summary>
    private static int _counter;

    /// <summary>
    /// Whether or not the hands are restored at the end of the event
    /// </summary>
    [DataField("permanent")]
    public bool Permanent = true;

    /// <summary>
    /// The amount of hands to be added/removed.
    /// </summary>
    [DataField("amount")]
    public int Amount;

    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var handSys = entity.System<HandsSystem>();

        var handsQuery = entity.GetEntityQuery<HandsComponent>();
        foreach (var player in players)
        {
            if (!handsQuery.TryGetComponent(player, out var hands))
                continue;

            var startCount = hands.SortedHands.Count;
            ChangeHandCount(player, Amount, hands, handSys, random);
            var endCount = hands.SortedHands.Count;

            if (!Permanent)
                entity.EnsureComponent<AddHandWareEventComponent>(player).Change += endCount - startCount;
        }
    }

    public override void End(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        if (Permanent)
            return;

        var handSys = entity.System<HandsSystem>();
        var handsQuery = entity.GetEntityQuery<HandsComponent>();
        var eventQuery = entity.GetEntityQuery<AddHandWareEventComponent>();
        foreach (var player in players)
        {
            if (!handsQuery.TryGetComponent(player, out var hands) || !eventQuery.TryGetComponent(player, out var wareEvent))
                continue;

            ChangeHandCount(player, -wareEvent.Change, hands, handSys, random);
            wareEvent.Change = 0;
        }
    }

    private void ChangeHandCount(EntityUid player, int amount, HandsComponent hands, HandsSystem handSys, IRobustRandom random)
    {
        if (amount < 0) //remove hands
        {
            for (var i = 0; i < Math.Abs(amount); i++)
            {
                if (!hands.SortedHands.Any())
                    break;
                var toRemove = random.Pick(hands.SortedHands);
                handSys.RemoveHand(player, toRemove, hands);
                _counter++;
            }
        }
        else // add hands
        {
            for (var i = 0; i < amount; i++)
            {
                var location = random.Pick(new[]{ HandLocation.Left, HandLocation.Right });
                handSys.AddHand(player, $"ware-hand-{_counter}", location, hands);
                _counter++;
            }
        }
    }
}
