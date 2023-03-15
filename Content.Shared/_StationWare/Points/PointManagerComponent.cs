using Robust.Shared.Network;

namespace Content.Shared._StationWare.Points;

/// <summary>
/// This is used for tracking players' individual points.
/// </summary>
/// <remarks>
/// Yes this is a singleton
/// No i don't give a shit
/// </remarks>
[RegisterComponent]
public sealed class PointManagerComponent : Component
{
    /// <summary>
    /// All of the points for the players
    /// </summary>
    [DataField("points")]
    public readonly Dictionary<NetUserId, PointInfo> Points = new();
}

/// <summary>
/// A little class used to associate a player's netUserId
/// with their name and point amount.
/// </summary>
[DataDefinition]
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
