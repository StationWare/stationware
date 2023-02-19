using Content.Server.Atmos.EntitySystems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Physics;
using Content.Shared.Spawners.Components;
using Content.Shared.Storage;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class SpawnRandomWareEvent : WareEvent
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
    [DataField("duration")]
    public float? Duration;

    /// <summary>
    /// Should the entities be spawned in clumps.
    /// If so, how large?
    /// </summary>
    [DataField("clumpSize")]
    public int? ClumpSize;

    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var stationSys = entity.System<StationSystem>();
        var grids = entity.GetEntityQuery<MapGridComponent>();
        foreach (var station in stationSys.Stations)
        {
            if (!entity.TryGetComponent<StationDataComponent>(station, out var stationData))
                continue;

            var grid = stationSys.GetLargestGrid(stationData);
            if (!grids.TryGetComponent(grid, out var mapGridComp))
                continue;

            var xform = entity.GetComponent<TransformComponent>(grid.Value);

            var positions = new List<EntityCoordinates>();
            for (var i = 0; i < (ClumpSize ?? 1); i++)
            {
                positions.Add(GetRandomPositionOnGrid(grid.Value, mapGridComp, xform, entity, random));
            }

            var spawns = EntitySpawnCollection.GetSpawns(Spawns);
            for (var i = 0; i < spawns.Count; i++)
            {
                var spawn = spawns[i];
                var position = ClumpSize == null
                    ? positions[i % positions.Count]
                    : positions[i % positions.Count % ClumpSize.Value];

                var ent = entity.SpawnEntity(spawn, position.Offset(random.NextVector2(0.2f)));
                if (Duration != null)
                    entity.EnsureComponent<TimedDespawnComponent>(ent).Lifetime = Duration.Value;
            }
        }
    }

    private EntityCoordinates GetRandomPositionOnGrid(EntityUid grid, MapGridComponent mapGridComp, TransformComponent xform, IEntityManager entity, IRobustRandom random)
    {
        var atmosphereSys = entity.System<AtmosphereSystem>();

        var gridBounds = mapGridComp.LocalAABB;

        for (var i = 0; i < 10; i++)
        {
            var randomX = random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = random.Next((int) gridBounds.Bottom, (int)gridBounds.Top);
            var tile = new Vector2i(randomX, randomY);

            // no air-blocked areas.
            if (atmosphereSys.IsTileSpace(grid, xform.MapUid, tile, mapGridComp: mapGridComp) ||
                atmosphereSys.IsTileAirBlocked(grid, tile, mapGridComp: mapGridComp))
            {
                continue;
            }

            // don't spawn inside of solid objects
            var physQuery = entity.GetEntityQuery<PhysicsComponent>();
            var valid = true;
            foreach (var ent in mapGridComp.GetAnchoredEntities(tile))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            return mapGridComp.GridTileToLocal(tile);
        }
        return xform.Coordinates;
    }
}
