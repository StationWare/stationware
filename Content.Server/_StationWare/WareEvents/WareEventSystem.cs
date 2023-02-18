using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents;

public sealed class WareEventSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private WareEventPrototype? _lastEventRan;

    public void RunWareEvent()
    {
        // TODO: maybe one day we'll want something more profound than this.
        var ev = _random.Pick(_prototype.EnumeratePrototypes<WareEventPrototype>().ToArray());

        var players = GetAllPlayers().ToArray();

        _chat.DispatchGlobalAnnouncement(Loc.GetString(ev.Announcement), announcementSound: ev.AnnouncementSound, colorOverride: Color.Fuchsia);

        _lastEventRan?.Event.End(players, EntityManager);
        ev.Event.Run(players, EntityManager, _random);
        _lastEventRan = ev;
    }

    private IEnumerable<EntityUid> GetAllPlayers()
    {
        foreach (var (actor, mobState) in EntityQuery<ActorComponent, MobStateComponent>())
        {
            var ent = actor.Owner;
            if (_mobState.IsDead(ent, mobState))
                continue;
            yield return ent;
        }
    }
}
