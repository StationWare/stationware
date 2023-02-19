using Content.Server.Polymorph.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Polymorph;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class PolymorphWareEvent : WareEvent
{
    /// <summary>
    /// The list of valid morphs that can be selected for this event.
    /// chosen per-person.
    /// </summary>
    [DataField("polymorphProtoypes", customTypeSerializer: typeof(PrototypeIdListSerializer<PolymorphPrototype>))]
    public List<string> PolymorphPrototypes = new();

    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var polymorphSys = entity.System<PolymorphableSystem>();
        foreach (var player in players)
        {
            polymorphSys.PolymorphEntity(player, random.Pick(PolymorphPrototypes));
        }
    }
}
