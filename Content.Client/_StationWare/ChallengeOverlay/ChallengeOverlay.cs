using Content.Client._StationWare.Points;
using Content.Shared._StationWare.Points;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._StationWare.ChallengeOverlay;

internal sealed class ChallengeOverlay : Overlay
{
    private readonly IPlayerManager _playerMgr;
    private readonly IEyeManager _eyeManager;

    private readonly PointSystem _point;

    private readonly ShaderInstance _shader;
    private readonly Font _font;
    private readonly Font _smallFont;

    public ChallengeOverlay(IEntityManager entity, IPrototypeManager proto, IResourceCache resourceCache, IEyeManager eyeManager, IPlayerManager player)
    {
        _eyeManager = eyeManager;
        _playerMgr = player;

        _point = entity.System<PointSystem>();

        ZIndex = 200;
        _shader = proto.Index<ShaderPrototype>("unshaded").Instance();
        _smallFont = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/warioware-inc/warioware-inc.ttf"), 6);
        _font = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/warioware-inc/warioware-inc.ttf"), 10);
    }

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public static bool DisplayChallengeText;
    public static string ChallengeText = string.Empty;
    public static Color TextColor = Color.White;

    public static Color ChallengeBoxColor = new(0.05f, 0.05f, 0.05f, 0.85f);

    public void UpdateText(string text, bool display, Color textColor)
    {
        DisplayChallengeText = display;
        ChallengeText = text;
        TextColor = textColor;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var aabb = args.WorldAABB;
        var width = args.ViewportBounds.Width;
        var height = args.ViewportBounds.Height;
        var scale = 2f;

        args.ScreenHandle.UseShader(_shader);

        // display the challenge announcement text
        if (DisplayChallengeText)
        {
            var dimensions = args.ScreenHandle.GetDimensions(_font, ChallengeText, scale);
            var textCoords = _eyeManager.WorldToScreen(aabb.Center) - dimensions / 2f - new Vector2(0, height / 2f * 0.8f);

            var backgroundDimensions = dimensions * new Vector2(1.05f, 1.1f);
            var offset = new Vector2((backgroundDimensions.X - dimensions.X) / 2f, 0);
            args.ScreenHandle.DrawRect(UIBox2.FromDimensions(textCoords - offset, backgroundDimensions), ChallengeBoxColor);
            args.ScreenHandle.DrawString(_font, textCoords, ChallengeText, scale, TextColor);
        }

        // update the point count
        var local = _playerMgr.LocalPlayer?.UserId;
        PointManagerComponent? manager = null;
        if (local != null && _point.TryGetPointManager(ref manager))
        {
            var points = _point.GetPoints(local, manager);
            var text = Loc.GetString("overlay-point-display", ("points", points));
            var countDimensions = args.ScreenHandle.GetDimensions(_font, text, scale);
            var pointScreenCoordinates = _eyeManager.WorldToScreen(aabb.Center)
                                         - new Vector2(width * 0.97f / 2f, (-height / 2f + countDimensions.Y / 2) * 0.95f);
            args.ScreenHandle.DrawString(_smallFont, pointScreenCoordinates, text, scale, Color.White);
        }

        args.ScreenHandle.UseShader(null);
    }
}
