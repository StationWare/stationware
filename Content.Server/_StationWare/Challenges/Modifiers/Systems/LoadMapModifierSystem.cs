using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class LoadMapModifierSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ResetPositionModifierSystem _resetPositionModifier = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeInitEvent>(OnChallengeInit);
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<LoadMapModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeInit(EntityUid uid, LoadMapModifierComponent component, ref ChallengeInitEvent args)
    {
        component.Map = _map.CreateMap();
        if (!_mapLoader.TryLoad(component.Map.Value, component.MapPath.ToString(), out _, new MapLoadOptions {LoadMap = true}))
            return;

        var mapEnt = _map.GetMapEntityId(component.Map.Value);
        Dirty(mapEnt);
    }

    private void OnChallengeStart(EntityUid uid, LoadMapModifierComponent component, ref ChallengeStartEvent args)
    {
        if (component.Map == null)
            return;

        var mapEnt = _map.GetMapEntityId(component.Map.Value);

        var validSpawns = new List<(EntityUid uid, Vector2)>();
        var query = EntityQueryEnumerator<MapPlayerSpawnerComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out _, out var xform))
        {
            if (xform.MapID == component.Map)
                validSpawns.Add((ent, _transform.GetWorldPosition(xform)));
        }

        if (!validSpawns.Any())
            return;

        foreach (var player in args.Players)
        {
            var (spawn, pos) = _random.Pick(validSpawns);
            var playerXform = Transform(player);
            _transform.SetParent(player, playerXform, mapEnt);
            _transform.SetWorldPosition(playerXform, pos);
            _transform.SetCoordinates(player, playerXform, new EntityCoordinates(spawn, 0, 0));
        }
    }

    private void OnChallengeEnd(EntityUid uid, LoadMapModifierComponent component, ref ChallengeEndEvent args)
    {
        _resetPositionModifier.ResetPositions(args.Players);

        if (component.Map != null)
            _map.DeleteMap(component.Map.Value);
    }
}
