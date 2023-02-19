using Content.Shared.Mobs;
using Robust.Shared.Configuration;

namespace Content.Server._StationWare.Mobs;

/// <summary>
/// This handles deleting entities once they die.
/// </summary>
public sealed class DeleteOnDeathSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(ref MobStateChangedEvent ev)
    {

    }
}
