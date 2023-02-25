using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for spawning entities repeatedly during a challenge
/// via <see cref="SpawnEntityModifierComponent"/>
/// </summary>
[RegisterComponent]
public sealed class RepeatSpawnEntityModifierComponent : Component
{
    /// <summary>
    /// The interval between each spawn
    /// </summary>
    [DataField("interval")]
    public TimeSpan Interval;

    /// <summary>
    /// When the next spawn will occur
    /// </summary>
    [DataField("nextSpawn", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextSpawn = TimeSpan.Zero;

    public bool Started = false;
}
