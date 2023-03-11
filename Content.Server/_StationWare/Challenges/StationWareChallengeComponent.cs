using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._StationWare.Challenges;

[RegisterComponent]
public sealed class StationWareChallengeComponent : Component
{
    /// <summary>
    /// Whether or not the default case for the challenge is winning
    /// </summary>
    [DataField("winByDefault")]
    public bool WinByDefault;

    /// <summary>
    /// Dictionary that stores whether or not each player has complete the challenge.
    /// </summary>
    [DataField("completions"), ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<NetUserId, bool?> Completions = new();

    /// <summary>
    /// When the challenge will begin
    /// </summary>
    [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? StartTime;

    /// <summary>
    /// When the challenge will end
    /// </summary>
    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? EndTime;

    /// <summary>
    /// The entity used to mark players as winners
    /// </summary>
    [DataField("winEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WinEffectPrototypeId = "WinEffect";

    /// <summary>
    /// The entity used to mark players as losers
    /// </summary>
    [DataField("loseEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string LoseEffectPrototypeId = "LoseEffect";
}
