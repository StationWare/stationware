namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is used for a challenge where a player
/// must say (or not say) a specific phrase.
/// </summary>
[RegisterComponent]
public sealed class SayPhraseModifierComponent : Component
{
    /// <summary>
    /// The phrase that must be said.
    /// </summary>
    [DataField("phrase", required: true)]
    public string Phrase = string.Empty;

    /// <summary>
    /// Whether or not you win or lose when you say the phrase.
    /// </summary>
    [DataField("wrongPhraseFail")]
    public bool WrongPhraseFail;
}

[RegisterComponent]
public sealed class SayPhrasePlayerComponent : Component
{
    [DataField("challenge")]
    public EntityUid Challenge;
}
