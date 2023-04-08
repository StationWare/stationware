using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Spawners.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class ResetPositionModifierSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ResetPositionModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeEnd(EntityUid uid, ResetPositionModifierComponent component, ref ChallengeEndEvent args)
    {
        ResetPositions(args.Players);
    }

    public void ResetPositions(List<EntityUid> players)
    {
        var validSpawns = EntityQuery<SpawnPointComponent, TransformComponent>()
            .Where(p => p.Item1.SpawnType == SpawnPointType.LateJoin)
            .Select(p => p.Item2).ToList();

        foreach (var player in players)
        {
            var spawn = _random.Pick(validSpawns).Coordinates;
            _transform.SetCoordinates(player, spawn);
        }
    }
}
