using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._StationWare.Challenges;

[RegisterComponent]
public sealed class StationWareChallengeComponent : Component
{
    [DataField("participants")]
    public Dictionary<IPlayerSession, EntityUid> Participants = new();

    [DataField("winByDefault")]
    public bool WinByDefault;

    [DataField("completions"), ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<IPlayerSession, bool?> Completions = new();

    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan EndTime;

    [DataField("winEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WinEffectPrototypeId = "WinEffect";

    [DataField("loseEffectPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string LoseEffectPrototypeId = "LoseEffect";
}
