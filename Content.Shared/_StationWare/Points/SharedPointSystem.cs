using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._StationWare.ChallengeOverlay;
using JetBrains.Annotations;
using Robust.Shared.Network;
using Robust.Shared.Players;
using Robust.Shared.Utility;

namespace Content.Shared._StationWare.Points;

public abstract class SharedPointSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedChallengeOverlaySystem _challengeOverlay = default!;

    public abstract bool TryGetPointManager([NotNullWhen(true)] ref PointManagerComponent? component);

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
        if (id == null || !TryGetPointManager(ref component))
            return 0;

        component.Points.TryGetValue(id.Value, out var points);
        return points?.Points ?? 0;
    }
    #endregion

    [PublicAPI]
    public void SetPoints(NetUserId id, int value, PointManagerComponent? component = null)
    {
        if (!TryGetPointManager(ref component))
            return;

        if (!TryGetPointInfo(id, component, out var info))
            return;
        info.Points = value;
        Dirty(component);
        _challengeOverlay.BroadcastText(string.Empty, false, Color.Black, id);
    }

    [PublicAPI]
    public void AdjustPoints(ICommonSession session, int delta, PointManagerComponent? component = null)
    {
        AdjustPoints(session.UserId, delta, component);
    }

    [PublicAPI]
    public void AdjustPoints(NetUserId id, int delta, PointManagerComponent? component = null)
    {
        if (!TryGetPointManager(ref component))
            return;

        if (!TryGetPointInfo(id, component, out var info))
            return;
        info.Points += delta;
        Dirty(component);
        _challengeOverlay.BroadcastText(string.Empty, false, Color.Black, id);
    }

    /// <summary>
    /// Gets the pointinfo class for a specified NetUserId.
    /// Creates a new one if it doesn't exist.
    /// </summary>
    [PublicAPI]
    public bool TryGetPointInfo(EntityUid uid, PointManagerComponent? component, [NotNullWhen(true)] out PointInfo? info)
    {
        info = null;
        if (!_player.TryGetSessionByEntity(uid, out var session))
            return false;
        return TryGetPointInfo(session.UserId, component, out info);
    }

    /// <summary>
    /// Gets the pointinfo class for a specified NetUserId.
    /// Creates a new one if it doesn't exist.
    /// </summary>
    [PublicAPI]
    public bool TryGetPointInfo(ICommonSession session, PointManagerComponent? component, [NotNullWhen(true)] out PointInfo? info)
    {
        return TryGetPointInfo(session.UserId, component, out info);
    }

    /// <summary>
    /// Gets the pointinfo class for a specified NetUserId.
    /// Creates a new one if it doesn't exist.
    /// </summary>
    public bool TryGetPointInfo(NetUserId id, PointManagerComponent? component, [NotNullWhen(true)] out PointInfo? info)
    {
        info = null;
        if (!TryGetPointManager(ref component))
            return false;

        EnsurePointInfo(component, id);
        info = component.Points[id];
        return true;
    }

    protected void EnsurePointInfo(PointManagerComponent? component, ICommonSession session)
    {
        EnsurePointInfo(component, session.UserId);
    }

    protected void EnsurePointInfo(PointManagerComponent? component, NetUserId id)
    {
        if (!TryGetPointManager(ref component))
            return;

        if (component.Points.ContainsKey(id))
            return;

        var valid = _player.Sessions.Where(s => s.UserId == id);
        var name = valid.FirstOrDefault()?.Name ?? "???";
        component.Points[id] = new PointInfo(name);
        Dirty(component);
    }

    public bool TryGetHighestScoringPlayer(PointManagerComponent? component, [NotNullWhen(true)] out KeyValuePair<NetUserId, PointInfo>? highest)
    {
        highest = null;
        if (!TryGetPointManager(ref component))
            return false;
        if (!component.Points.Any())
            return false;

        highest = component.Points.MaxBy(p => p.Value.Points);
        return true;
    }

    /// <summary>
    /// Creates a formatted message of a scoreboard of all players, ordered from greatest to least.
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public FormattedMessage GetPointScoreBoard(PointManagerComponent? component = null)
    {
        var msg = new FormattedMessage();
        if (!TryGetPointManager(ref component))
            return msg;

        var sorted = component.Points.Values.OrderByDescending(p => p.Points);
        var placement = 0;
        var placementThreshold = int.MaxValue;
        foreach (var pointInfo in sorted)
        {
            if (pointInfo.Points < placementThreshold)
                placement++;
            placementThreshold = pointInfo.Points;

            msg.AddMarkup(Loc.GetString("stationware-report-score",
                ("place", placement),
                ("name", pointInfo.Name),
                ("points", pointInfo.Points)));
            msg.PushNewline();
        }
        return msg;
    }
}
