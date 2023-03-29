namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for getting players to speak
/// </summary>
[RegisterComponent]
public sealed class SayAnythingModifierComponent : Component
{
    [DataField("shouldSpeak")] public bool ShouldSpeak = true;
}

[RegisterComponent]
public sealed class SayAnythingPlayerComponent : Component
{
    [DataField("challenge")] public EntityUid Challenge;
}
