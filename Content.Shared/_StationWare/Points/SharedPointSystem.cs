using System.Linq;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Players;
using Robust.Shared.Utility;

namespace Content.Shared._StationWare.Points;

public abstract class SharedPointSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    /// <summary>
    /// Gets the point manager for the round.
    /// If none exists, creates one.
    /// </summary>
    /// <returns></returns>
    public PointManagerComponent GetPointManager()
    {
        var query = EntityQuery<PointManagerComponent>(true).ToList();
        if (!query.Any())
        {
            var manager = Spawn(null, MapCoordinates.Nullspace);
            var managerComp = EnsureComp<PointManagerComponent>(manager);
            return managerComp;
        }
        return query.First();
    }

    #region GetPoints
    [PublicAPI]
    public int GetPoints(EntityUid? uid, PointManagerComponent? component = null)
    {
        if (uid == null)
            return 0;

        _player.TryGetSessionByEntity(uid.Value, out var session);
        return GetPoints(session?.UserId, component);

    }
    [PublicAPI]
    public int GetPoints(ICommonSession? session, PointManagerComponent? component = null)
    {
        return GetPoints(session?.UserId, component);
    }

    public int GetPoints(NetUserId? id, PointManagerComponent? component = null)
    {
        if (id == null)
            return 0;

        component ??= GetPointManager();
        component.Points.TryGetValue(id.Value, out var points);
        return points?.Points ?? 0;
    }
    #endregion

    [PublicAPI]
    public void SetPoints(NetUserId id, int value, PointManagerComponent? component = null)
    {
        component ??= GetPointManager();
        var info = GetPointInfo(component, id);
        info.Points = value;
        Dirty(component);
    }

    [PublicAPI]
    public void AdjustPoints(ICommonSession session, int delta, PointManagerComponent? component = null)
    {
        AdjustPoints(session.UserId, delta, component);
    }

    [PublicAPI]
    public void AdjustPoints(NetUserId id, int delta, PointManagerComponent? component = null)
    {
        component ??= GetPointManager();
        var info = GetPointInfo(component, id);
        info.Points += delta;
        Dirty(component);
    }

    /// <summary>
    /// Gets the pointinfo class for a specified NetUserId.
    /// Creates a new one if it doesn't exist.
    /// </summary>
    public PointInfo GetPointInfo(PointManagerComponent? component, NetUserId id)
    {
        component ??= GetPointManager();
        EnsurePointInfo(component, id);
        return component.Points[id];
    }

    protected void EnsurePointInfo(PointManagerComponent? component, ICommonSession id)
    {
        EnsurePointInfo(component, id.UserId);
    }

    protected void EnsurePointInfo(PointManagerComponent? component, NetUserId id)
    {
        component ??= GetPointManager();

        if (component.Points.ContainsKey(id))
            return;

        var valid = _player.Sessions.Where(s => s.UserId == id);
        var name = valid.FirstOrDefault()?.Name ?? "Unknown";
        component.Points[id] = new PointInfo(name);
        Dirty(component);
    }

    /// <summary>
    /// Creates a formatted message of a scoreboard of all players, ordered from greatest to least.
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public FormattedMessage GetPointScoreBoard(PointManagerComponent? component = null)
    {
        component ??= GetPointManager();
        var sorted = component.Points.Values.OrderByDescending(p => p.Points);

        var msg = new FormattedMessage();
        var placement = 0;
        var placementThreshold = int.MaxValue;
        foreach (var pointInfo in sorted)
        {
            if (pointInfo.Points < placementThreshold)
                placement++;
            else
                placement = pointInfo.Points;

            msg.AddMarkup(Loc.GetString("stationware-report-score",
                ("place", placement),
                ("name", pointInfo.Name),
                ("points", pointInfo.Points)));
        }
        return msg;
    }
}
