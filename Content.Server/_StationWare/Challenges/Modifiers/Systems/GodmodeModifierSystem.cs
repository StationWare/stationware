using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Damage.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class GodmodeModifierSystem : EntitySystem
{
    [Dependency] private readonly GodmodeSystem _godmode = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GodmodeModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<GodmodeModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, GodmodeModifierComponent component, ref ChallengeStartEvent args)
    {
        foreach (var ent in args.Players)
        {
            _godmode.EnableGodmode(ent);
        }
    }

    private void OnChallengeEnd(EntityUid uid, GodmodeModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var ent in args.Players)
        {
            _godmode.DisableGodmode(ent);
        }
    }
}
