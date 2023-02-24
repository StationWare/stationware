namespace Content.Server._StationWare.Challenges.Modifiers.Components;

/// <summary>
/// This is for entities that make you win/lose a challenge
/// when you interact with them.
/// </summary>
[RegisterComponent]
public sealed class InteractionWinComponent : Component
{
    /// <summary>
    /// Whether or not you win or lose on interact
    /// </summary>
    [DataField("winOnInteract")]
    public bool WinOnInteract = true;
}
