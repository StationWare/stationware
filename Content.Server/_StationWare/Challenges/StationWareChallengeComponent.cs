using Robust.Server.Player;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

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
}
