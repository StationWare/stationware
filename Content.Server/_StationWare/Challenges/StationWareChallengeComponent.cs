using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._StationWare.Challenges;

[RegisterComponent]
public sealed class StationWareChallengeComponent : Component
{
    /// <summary>
    /// The players and their entities who are involved in the challenge.
    /// </summary>
    [DataField("participants")]
    public Dictionary<IPlayerSession, EntityUid> Participants = new();

    /// <summary>
    /// Whether or not the default case for the challenge is winning
    /// </summary>
    [DataField("winByDefault")]
    public bool WinByDefault;

    /// <summary>
    /// Dictionary that stores whether or not each player has complete the challenge.
    /// </summary>
    [DataField("completions"), ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<IPlayerSession, bool?> Completions = new();

    [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? StartTime;

    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan EndTime;

    [DataField("winEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WinEffectPrototypeId = "WinEffect";

    [DataField("loseEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string LoseEffectPrototypeId = "LoseEffect";
}
