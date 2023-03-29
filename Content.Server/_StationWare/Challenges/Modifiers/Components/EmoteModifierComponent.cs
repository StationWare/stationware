using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for getting players to emote
/// </summary>
[RegisterComponent]
public sealed class EmoteModifierComponent : Component
{
    [DataField("shouldEmote")]
    public bool ShouldEmote = true;

    [DataField("targetEmotes", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EmotePrototype>))]
    public HashSet<string>? TargetEmotes;

    [DataField("failOnIncorrectEmote")]
    public bool FailOnIncorrectEmote;
}

[RegisterComponent]
public sealed class EmotePlayerComponent : Component
{
    [DataField("challenge")] public EntityUid Challenge;
}
