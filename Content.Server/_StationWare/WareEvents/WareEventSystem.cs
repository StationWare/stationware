using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents;

public sealed partial class WareEventSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    /// <summary>
    /// The last event we ran. Yes it's stinky storing this here but idgaf
    /// </summary>
    private WareEventPrototype? _lastEventRan;

    private List<EntityUid> _lastPlayersAffected = new();

    public override void Initialize()
    {
        base.Initialize();

        InitializeCommands();
    }

    public void RunRandomWareEvent()
    {
        // TODO: maybe one day we'll want something more profound than this.
        var ev = _random.Pick(_prototype.EnumeratePrototypes<WareEventPrototype>().ToArray());
        RunWareEvent(ev);
    }

    public void RunWareEvent(WareEventPrototype ev)
    {
        var players = GetAllPlayers().ToList();

        if (!players.Any())
            return;

        _random.Shuffle(players);

        // if we specify, cut the number of players we affect.
        if (ev.Event.PlayersToAffect.HasValue)
            players = players.Take(Math.Min(ev.Event.PlayersToAffect.Value, players.Count)).ToList();

        // announce
        _chat.DispatchGlobalAnnouncement(Loc.GetString(ev.Announcement), announcementSound: ev.AnnouncementSound, colorOverride: Color.Fuchsia);

        _lastEventRan?.Event.End(_lastPlayersAffected, EntityManager, _random);

        ev.Event.Run(players, EntityManager, _random);

        var eventRanEvent = new WareEventRanEvent();
        RaiseLocalEvent(ref eventRanEvent);

        _lastEventRan = ev;
        _lastPlayersAffected = players;

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

[ByRefEvent]
public readonly record struct WareEventRanEvent;
