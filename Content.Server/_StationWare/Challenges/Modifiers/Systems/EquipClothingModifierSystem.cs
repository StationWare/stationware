using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

/// <summary>
/// This handles <see cref="EquipClothingModifierComponent"/>
/// </summary>
public sealed class EquipClothingModifierSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EquipClothingModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<EquipClothingModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, EquipClothingModifierComponent component, ref ChallengeStartEvent args)
    {
        if (!args.Players.Any())
            return;

        var playerAmount = (int) Math.Clamp(MathF.Round(args.Players.Count * component.ReceivingPercentage), 1, args.Players.Count);

        foreach (var player in args.Players.Take(playerAmount))
        {
            var xform = Transform(player);
            var slots = _inventory.GetSlots(player);
            var spawns = EntitySpawnCollection.GetSpawns(component.Spawns, _random);
            foreach (var spawn in spawns)
            {
                var clothing = Spawn(spawn, xform.Coordinates);
                if (slots.Any(slot => _inventory.CanEquip(player, clothing, slot.Name, out _, slot) &&
                                      _inventory.TryEquip(player, clothing, slot.Name, true, true)))
                {
                    component.SpawnedEntities.Add(clothing);
                }
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, EquipClothingModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var ent in component.SpawnedEntities)
        {
            Del(ent);
        }
    }
}
