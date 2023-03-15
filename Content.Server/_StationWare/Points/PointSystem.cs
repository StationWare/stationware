using Content.Server._StationWare.Challenges;
using Content.Shared._StationWare.Points;
using Robust.Server.GameStates;
using Robust.Shared.GameStates;

namespace Content.Server._StationWare.Points;

public sealed class PointSystem : SharedPointSystem
{
    [Dependency] private readonly PVSOverrideSystem _pvs = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointManagerComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<PointManagerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PlayerChallengeStateSetEvent>(OnPlayerChallengeStateSet);
    }

    private void OnGetState(EntityUid uid, PointManagerComponent component, ref ComponentGetState args)
    {
        args.State = new PointManagerComponentState(component.Points);
    }

    private void OnInit(EntityUid uid, PointManagerComponent component, ComponentInit args)
    {
        _pvs.AddGlobalOverride(uid);
    }

    private void OnPlayerChallengeStateSet(ref PlayerChallengeStateSetEvent ev)
    {
        var manager = GetPointManager();
        EnsurePointInfo(manager, ev.Player);
        if (ev.Won)
            AdjustPoints(ev.Player, 1, manager);
    }
}
