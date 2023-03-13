using Content.Server.Body.Components;
using Content.Shared.CCVar;
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
        SubscribeLocalEvent<BeingGibbedEvent>(OnBeingGibbed);
        _configuration.OnValueChanged(CCVars.StationWareMobsDeleteDeadBodies, e => _enabled = e, true);
    }

    private bool _enabled;

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (!_enabled)
            return;

        if (ev.NewMobState != MobState.Dead)
            return;

        QueueDel(ev.Target);
    }

    private void OnBeingGibbed(BeingGibbedEvent ev)
    {
        if (!_enabled)
            return;

        foreach (var part in ev.GibbedParts)
        {
            if (!Deleted(part) && !Terminating(part))
                QueueDel(part);
        }
    }
}
