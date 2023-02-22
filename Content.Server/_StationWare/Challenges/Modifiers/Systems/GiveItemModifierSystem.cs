using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Hands.Components;
using Content.Server.Hands.Systems;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class GiveItemModifierSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GiveItemModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<GiveItemModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, GiveItemModifierComponent component, ref ChallengeStartEvent args)
    {
        var toSelect = (float) args.Players.Count;
        if (component.PopulationPercentage is { } percentage)
            toSelect = percentage * toSelect;

        if (component.PopulationAmount is { } amount)
            toSelect = Math.Min(args.Players.Count, amount);

        var playersToUse = args.Players.Take((int) Math.Round(toSelect));

        var handsQuery = GetEntityQuery<HandsComponent>();
        foreach (var player in playersToUse)
        {
            if (!handsQuery.TryGetComponent(player, out var handsComponent))
                continue;

            var xform = Transform(player);
            var spawns = EntitySpawnCollection.GetSpawns(component.Items, _random);
            foreach (var spawnProto in spawns)
            {
                var spawnedEntity = Spawn(spawnProto, xform.Coordinates);
                _hands.TryPickupAnyHand(player, spawnedEntity, false, handsComp: handsComponent);
                component.ProvidedItems.Add(spawnedEntity);
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, GiveItemModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var item in component.ProvidedItems)
        {
            Del(item);
        }
    }
}
