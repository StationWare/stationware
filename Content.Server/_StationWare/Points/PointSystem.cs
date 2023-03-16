using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._StationWare.Challenges;
using Content.Shared._StationWare.Points;
using Robust.Server.GameStates;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Server._StationWare.Points;

public sealed class PointSystem : SharedPointSystem
{
    [Dependency] private readonly PVSOverrideSystem _pvs = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointManagerComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<PointManagerComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<PlayerChallengeStateSetEvent>(OnPlayerChallengeStateSet);
    }

    private void OnGetState(EntityUid uid, PointManagerComponent component, ref ComponentGetState args)
    {
        args.State = new PointManagerComponentState(component.Points);
    }

    private void OnInit(EntityUid uid, PointManagerComponent component, ComponentStartup args)
    {
        _pvs.AddGlobalOverride(uid);
    }

    private void OnPlayerChallengeStateSet(ref PlayerChallengeStateSetEvent ev)
    {
        PointManagerComponent? manager = null;
        if (!TryGetPointManager(ref manager))
            return;

        EnsurePointInfo(manager, ev.Player);
        if (ev.Won)
            AdjustPoints(ev.Player, 1, manager);
    }

    public override bool TryGetPointManager([NotNullWhen(true)] ref PointManagerComponent? component)
    {
        if (component != null)
            return true;

        var query = EntityQuery<PointManagerComponent>().ToList();
        component = !query.Any() ? CreatePointManager() : query.First();
        return true;
    }

    public PointManagerComponent CreatePointManager()
    {
        var manager = Spawn(null, MapCoordinates.Nullspace);
        return EnsureComp<PointManagerComponent>(manager);
    }
}
