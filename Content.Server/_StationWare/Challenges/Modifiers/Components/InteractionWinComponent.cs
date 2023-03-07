namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is for entities that make you win/lose a challenge
/// when you interact with them.
/// </summary>
[RegisterComponent]
public sealed class InteractionWinComponent : Component
{
    /// <summary>
    /// The chance that you win when you interact.
    /// </summary>
    [DataField("winChance")]
    public float WinChance = 1;

    /// <summary>
    /// Whether or not you fail the challenge if <see cref="WinChance"/> fails
    /// </summary>
    [DataField("failOnNoWin")]
    public bool FailOnNoWin;
}
