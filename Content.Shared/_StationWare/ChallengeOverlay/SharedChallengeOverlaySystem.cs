using Robust.Shared.Serialization;

namespace Content.Shared._StationWare.ChallengeOverlay;

/// <inheritdoc/>
public abstract class SharedChallengeOverlaySystem : EntitySystem
{
    [Serializable, NetSerializable]
    public sealed class UpdateChallengeText : EntityEventArgs
    {
        public string Text;
        public bool Shown;
        public Color TextColor;

        public UpdateChallengeText(string text, bool shown, Color textColor)
        {
            Text = text;
            Shown = shown;
            TextColor = textColor;
        }
    }
}
