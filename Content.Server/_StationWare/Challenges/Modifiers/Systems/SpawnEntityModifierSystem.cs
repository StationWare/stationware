using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Physics;
using Content.Shared.Storage;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class SpawnEntityModifierSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly StationSystem _station = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnEntityModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<SpawnEntityModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
        SubscribeLocalEvent<RepeatSpawnEntityModifierComponent, ChallengeStartEvent>(OnRepeatChallengeStart);
    }

    private void OnChallengeStart(EntityUid uid, SpawnEntityModifierComponent component, ref ChallengeStartEvent args)
    {
        SpawnEntities(uid, component, args.Players.Count);
    }

    public void SpawnEntities(EntityUid uid, SpawnEntityModifierComponent component, int players)
    {
        var gridQuery = GetEntityQuery<MapGridComponent>();
        foreach (var station in _station.Stations)
        {
            if (!TryComp<StationDataComponent>(station, out var stationData))
                continue;

            var grid = _station.GetLargestGrid(stationData);
            if (!gridQuery.TryGetComponent(grid, out var gridComp))
                continue;

            var spawns = EntitySpawnCollection.GetSpawns(component.Spawns);
            if (component.SpawnPerPlayer)
            {
                var amount = (int) MathF.Round(players * component.SpawnPerPlayerMultiplier);
                spawns = spawns.Take(Math.Min(spawns.Count, amount)).ToList();
            }

            var positions = new List<EntityCoordinates>();
            for (var i = 0; i < spawns.Count / (component.ClumpSize ?? 1); i++)
            {
                positions.Add(GetRandomPositionOnGrid(grid.Value, gridComp, component.SpawnLocationScalar));
            }

            for (var i = 0; i < spawns.Count; i++)
            {
                var spawn = spawns[i];
                var position = positions[i % positions.Count];
                var ent = Spawn(spawn, position.Offset(_random.NextVector2(0.2f)));
                component.SpawnedEntities.Add(ent);
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, SpawnEntityModifierComponent component, ref ChallengeEndEvent args)
    {
        if (!component.DeleteAtEnd)
            return;

        foreach (var ent in component.SpawnedEntities)
        {
            Del(ent);
        }
    }

    private EntityCoordinates GetRandomPositionOnGrid(EntityUid grid, MapGridComponent mapGridComp, float scalar)
    {
        var xform = Transform(grid);
        var gridBounds = mapGridComp.LocalAABB.Scale(scalar);

        for (var i = 0; i < 10; i++)
        {
            var randomX = _random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = _random.Next((int) gridBounds.Bottom, (int)gridBounds.Top);
            var tile = new Vector2i(randomX, randomY);

            // no air-blocked areas.
            if (_atmosphere.IsTileSpace(grid, xform.MapUid, tile, mapGridComp: mapGridComp) ||
                _atmosphere.IsTileAirBlocked(grid, tile, mapGridComp: mapGridComp))
            {
                continue;
            }

            // don't spawn inside of solid objects
            var physQuery = GetEntityQuery<PhysicsComponent>();
            var markerQuery = GetEntityQuery<SpawnBlockMarkerComponent>();
            var valid = true;
            foreach (var ent in mapGridComp.GetAnchoredEntities(tile))
            {
                if (markerQuery.HasComponent(ent))
                {
                    valid = false;
                    break;
                }

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

    private void OnRepeatChallengeStart(EntityUid uid, RepeatSpawnEntityModifierComponent component, ref ChallengeStartEvent args)
    {
        component.NextSpawn = _timing.CurTime + component.Interval;
        component.Started = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (timer, spawn, challenge) in EntityQuery<RepeatSpawnEntityModifierComponent, SpawnEntityModifierComponent, StationWareChallengeComponent>())
        {
            if (!timer.Started)
                continue;
            if (_timing.CurTime < timer.NextSpawn)
                continue;
            timer.NextSpawn = _timing.CurTime + timer.Interval;
            SpawnEntities(spawn.Owner, spawn, challenge.Completions.Count);
        }
    }
}
