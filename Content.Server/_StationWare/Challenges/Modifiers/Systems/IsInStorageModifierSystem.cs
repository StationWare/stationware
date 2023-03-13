using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Server.Containers;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

/// <summary>
/// This handles checking all players whether they're inside of an entity storage.
/// </summary>
public sealed class IsInsideStorageModifierSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWare = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<IsInsideStorageModifierComponent, BeforeChallengeEndEvent>(BeforeChallengeEndEvent);
    }

    private void BeforeChallengeEndEvent(EntityUid uid, IsInsideStorageModifierComponent component,
        ref BeforeChallengeEndEvent args)
    {
        foreach (var player in args.Players)
        {
            if (_containerSystem.IsEntityInContainer(player))
                _stationWare.SetPlayerChallengeState(player, uid, true);
        }
    }
}
