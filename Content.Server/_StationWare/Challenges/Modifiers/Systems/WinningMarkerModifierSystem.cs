using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class WinningMarkerModifierSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WinningMarkerModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<WinningMarkerModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
        SubscribeLocalEvent<WinningMarkerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WinningMarkerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WinningMarkerComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<WinningMarkerComponent, InteractHandEvent>(OnAfterInteractUsing);
    }

    private void OnChallengeStart(EntityUid uid, WinningMarkerModifierComponent component, ref ChallengeStartEvent args)
    {
        var toSelect = Math.Min(args.Players.Count, component.Amount);
        foreach (var player in args.Players.Take(toSelect))
        {
            var winning = _componentFactory.GetComponent<WinningMarkerComponent>();
            winning.Challenge = uid;
            winning.Owner = player;
            EntityManager.AddComponent(player, winning);
        }
    }

    private void OnBeforeChallengeEnd(EntityUid uid, WinningMarkerModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        foreach (var winning in EntityQuery<WinningMarkerComponent>())
        {
            var ent = winning.Owner;
            if (winning.Challenge != uid)
                continue;

            _stationWareChallenge.SetPlayerChallengeState(ent, uid, true, args.Component);
            RemComp(ent, winning);
        }
    }

    private void OnStartup(EntityUid uid, WinningMarkerComponent component, ComponentStartup args)
    {
        var xform = Transform(uid);
        if (!TryComp<WinningMarkerModifierComponent>(component.Challenge, out var modifier))
            return;
        var proto = modifier.MarkerEffectPrototype;
        var ent = Spawn(proto, xform.Coordinates);
        _transform.SetParent(ent, uid);
        component.Marker = ent;
    }

    private void OnShutdown(EntityUid uid, WinningMarkerComponent component, ComponentShutdown args)
    {
        if (component.Marker != null)
            Del(component.Marker.Value);
    }

    private void OnAttacked(EntityUid uid, WinningMarkerComponent component, AttackedEvent args)
    {
        TryTransferWinningMarker(args.User, uid, component);
    }

    private void OnAfterInteractUsing(EntityUid uid, WinningMarkerComponent component, InteractHandEvent args)
    {
        TryTransferWinningMarker(args.User, uid, component);
    }

    private void TryTransferWinningMarker(EntityUid user, EntityUid uid, WinningMarkerComponent component)
    {
        if (HasComp<WinningMarkerComponent>(user))
            return;

        var winning = _componentFactory.GetComponent<WinningMarkerComponent>();
        winning.Challenge = component.Challenge;
        winning.Owner = user;
        EntityManager.AddComponent(user, winning);
        RemComp(uid, component);

    }
}
