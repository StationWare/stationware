using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._StationWare.ChallengeOverlay;

/// <inheritdoc/>
public abstract class SharedChallengeOverlaySystem : EntitySystem
{
    public virtual void BroadcastText(string text, bool shown, Color textColor, NetUserId id)
    {

    }

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
