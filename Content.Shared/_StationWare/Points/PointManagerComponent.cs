using Robust.Shared.Network;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._StationWare.Points;

/// <summary>
/// This is used for tracking players' individual points.
/// </summary>
/// <remarks>
/// Yes this is a singleton
/// No i don't give a shit
/// </remarks>
[RegisterComponent, NetworkedComponent]
public sealed class PointManagerComponent : Component
{
    /// <summary>
    /// All of the points for the players
    /// </summary>
    [DataField("points")]
    public Dictionary<NetUserId, PointInfo> Points = new();
}

[Serializable, NetSerializable]
public sealed class PointManagerComponentState : ComponentState
{
    public readonly Dictionary<NetUserId, PointInfo> Points;

    public PointManagerComponentState(Dictionary<NetUserId, PointInfo> points)
    {
        Points = points;
    }
}

/// <summary>
/// A little class used to associate a player's netUserId
/// with their name and point amount.
/// </summary>
[Serializable]
public sealed class PointInfo
{
    /// <summary>
    /// The name of the player associated with the points
    /// </summary>
    [DataField("name")]
    public string Name;

    /// <summary>
    /// The actual points themselves
    /// </summary>
    [DataField("points")]
    public int Points;

    public PointInfo(string name, int points = 0)
    {
        Name = name;
        Points = points;
    }
}
