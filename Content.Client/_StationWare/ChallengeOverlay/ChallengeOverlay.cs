using Content.Client.Administration.Systems;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Client._StationWare.ChallengeOverlay;

internal sealed class ChallengeOverlay : Overlay
{
    private readonly ChallengeOverlaySystem _system;
    private readonly IEyeManager _eyeManager;
    private readonly Font _font;

    public ChallengeOverlay(ChallengeOverlaySystem system, IResourceCache resourceCache, IEyeManager eyeManager)
    {
        _system = system;
        _eyeManager = eyeManager;
        ZIndex = 200;
        _font = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/warioware-inc/warioware-inc.ttf"), 10);
    }

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public static bool DisplayChallengeText;
    public static string ChallengeText = "It's stationing time!";
    public static Color TextColor = Color.Aquamarine;

    public void UpdateText(string text, bool display, Color textColor)
    {
        DisplayChallengeText = display;
        ChallengeText = text;
        TextColor = textColor;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var aabb = args.WorldAABB;
        var scale = 2f;
        if (DisplayChallengeText)
        {
            var dimensions = args.ScreenHandle.GetDimensions(_font, ChallengeText, scale);

            // challenge text
            var challengeScreenCoordinates = (_eyeManager.WorldToScreen(aabb.Center) - dimensions / 2f) + new Vector2(0, 250);
            args.ScreenHandle.DrawString(_font, challengeScreenCoordinates, ChallengeText, scale, TextColor);
        }

        // cause emo stored all the points in the gamerule which is yucky
        // var pointScreenCoordinates = _eyeManager.WorldToScreen(aabb.BottomLeft);
        // args.ScreenHandle.DrawString(_font, pointScreenCoordinates, "Points: 0", scale, Color.MediumAquamarine);

    }
}
