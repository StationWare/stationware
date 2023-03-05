using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Server.Body.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class GibOnFailModifierSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GibOnFailModifierComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
    }

    private void OnChallengeStateSet(EntityUid uid, GibOnFailModifierComponent component, ref PlayerChallengeStateSetEvent args)
    {
        if (args.Won)
            return;
        if (args.Player.AttachedEntity is not { } ent)
            return;
        _body.GibBody(ent, true, deleteItems: true);
    }
}
