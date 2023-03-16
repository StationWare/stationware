using Content.Shared._StationWare.ChallengeOverlay;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client._StationWare.ChallengeOverlay;

/// <inheritdoc/>
public sealed class ChallengeOverlaySystem : SharedChallengeOverlaySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    private ChallengeOverlay _challengeOverlay = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        InitializeOverlay();
        ChallengeOverlayOn();

        SubscribeNetworkEvent<UpdateChallengeText>(OnChallengeTextUpdate);

        _challengeOverlay.UpdateText("", false, Color.Black);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        ChallengeOverlayOff();
    }

    private void OnChallengeTextUpdate(UpdateChallengeText ev)
    {
        _challengeOverlay.UpdateText(ev.Text, ev.Shown, ev.TextColor);
    }

    private void ChallengeOverlayOn()
    {
        if (_overlayManager.HasOverlay<ChallengeOverlay>())
            return;
        _overlayManager.AddOverlay(_challengeOverlay);
    }

    private void ChallengeOverlayOff()
    {
        _overlayManager.RemoveOverlay(_challengeOverlay);
    }

    private void InitializeOverlay()
    {
        _challengeOverlay = new ChallengeOverlay(EntityManager, _prototype, _resourceCache, _eyeManager, _playerManager);
    }
}
