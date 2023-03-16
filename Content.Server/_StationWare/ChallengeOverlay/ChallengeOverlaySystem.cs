using Content.Shared._StationWare.ChallengeOverlay;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._StationWare.ChallengeOverlay;

public sealed class ChallengeOverlaySystem : SharedChallengeOverlaySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void BroadcastText(string text, bool shown, Color textColor, NetUserId id)
    {
        if (_playerManager.TryGetSessionById(id, out var session))
            BroadcastText(text, shown, textColor, session);
    }

    public void BroadcastText(string text, bool shown, Color textColor, IPlayerSession? session = null)
    {
        var filter = Filter.Empty();

        if (session != null)
        {
            filter.AddPlayer(session);
        }
        else
        {
            filter.AddAllPlayers();
        }

        RaiseNetworkEvent(
            new UpdateChallengeText(text, shown, textColor),
            filter,
            false
        );
    }
}
