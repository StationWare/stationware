using Content.Server.Hands.Components;
using Content.Server.Hands.Systems;
using Content.Shared._StationWare.WareEvents;
using Content.Shared.Spawners.Components;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._StationWare.WareEvents.Events;

public sealed class GiveItemWareEvent : WareEvent
{
    /// <summary>
    /// What entities will be spawned and given to the player.
    /// </summary>
    [DataField("spawns", required: true)]
    public List<EntitySpawnEntry> Spawns = default!;

    /// <summary>
    /// How long will the item last?
    /// if null, has no limit.
    /// </summary>
    [DataField("itemDuration")]
    public float? ItemDuration;

    /// <summary>
    /// The max amount of items that the event will spawn
    /// </summary>
    [DataField("maxItems")]
    public int MaxItems = int.MaxValue;

    /// <inheritdoc/>
    public override void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {
        var hands = entity.System<HandsSystem>();

        var handsQuery = entity.GetEntityQuery<HandsComponent>();
        var count = 0;
        foreach (var player in players)
        {
            var xform = entity.GetComponent<TransformComponent>(player);

            if (!handsQuery.TryGetComponent(player, out var handsComponent))
                continue;

            var spawns = EntitySpawnCollection.GetSpawns(Spawns, random);
            foreach (var spawnProto in spawns)
            {
                if (count > MaxItems)
                    break;

                var spawnedEntity = entity.SpawnEntity(spawnProto, xform.Coordinates);
                count++;
                if (ItemDuration != null)
                {
                    var timedDespawn = entity.EnsureComponent<TimedDespawnComponent>(spawnedEntity);
                    timedDespawn.Lifetime = ItemDuration.Value;
                }
                hands.TryPickupAnyHand(player, spawnedEntity, false, handsComp: handsComponent);
            }
        }
    }
}
