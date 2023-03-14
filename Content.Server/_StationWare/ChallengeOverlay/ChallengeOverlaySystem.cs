using Content.Shared._StationWare.ChallengeOverlay;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server._StationWare.ChallengeOverlay;

public sealed class ChallengeOverlaySystem : SharedChallengeOverlaySystem
{
    public void BroadcastText(string text, bool shown, Color textColor, IPlayerSession? session)
    {
        var filter = Filter.Empty();

        if (session == null)
        {
            filter.AddAllPlayers();
        }
        else
        {
            filter.AddPlayer(session);
        }

        RaiseNetworkEvent(
            new UpdateChallengeText(text, shown, textColor),
            filter,
            false
        );
    }
}
