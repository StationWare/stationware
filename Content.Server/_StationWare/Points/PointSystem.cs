using Content.Server._StationWare.Challenges;
using Content.Shared._StationWare.Points;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;

namespace Content.Server._StationWare.Points;

public sealed class PointSystem : SharedPointSystem
{
    [Dependency] private readonly PVSOverrideSystem _pvs = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointManagerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PlayerChallengeStateSetEvent>(OnPlayerChallengeStateSet);
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
