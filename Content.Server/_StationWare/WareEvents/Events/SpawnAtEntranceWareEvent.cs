using System.Linq;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Spawners.Components;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class SpawnAtEntranceWareEvent : WareEvent
{
    /// <summary>
    /// What entities will be spawned at the bus entrance
    /// </summary>
    [DataField("spawns", required: true)]
    public List<EntitySpawnEntry> Spawns = default!;

    /// <summary>
    /// How long will the entity last?
    /// if null, has no limit.
    /// </summary>
    [DataField("itemDuration")]
    public float? ItemDuration;

    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var stationSys = entity.System<StationSystem>();
        var spawnPoints = entity.EntityQuery<SpawnPointComponent>().ToList();
        foreach (var station in stationSys.Stations)
        {
            EntityUid? selectedSpawnPoint = null;
            TransformComponent? selectedSpawnXform = null;
            foreach (var spawnPoint in spawnPoints)
            {
                var spawnPointEnt = spawnPoint.Owner;
                if (spawnPoint.SpawnType != SpawnPointType.LateJoin)
                    continue;

                var tempXform = entity.GetComponent<TransformComponent>(spawnPointEnt);
                if (stationSys.GetOwningStation(spawnPointEnt) != station)
                    continue;

                selectedSpawnPoint = spawnPointEnt;
                selectedSpawnXform = tempXform;
                break;
            }

            if (selectedSpawnPoint == null)
                continue;

            foreach (var spawn in EntitySpawnCollection.GetSpawns(Spawns, random))
            {
                var ent = entity.SpawnEntity(spawn, selectedSpawnXform!.Coordinates
                    .Offset(random.NextVector2(0.2f)));
                if (ItemDuration != null)
                    entity.EnsureComponent<TimedDespawnComponent>(ent).Lifetime = ItemDuration.Value;
            }
        }
    }
}
