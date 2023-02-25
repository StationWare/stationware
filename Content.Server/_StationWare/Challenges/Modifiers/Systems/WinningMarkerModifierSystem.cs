using System.Linq;
using Content.Server._StationWare.Challenges.Modifiers.Components;
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
        foreach (var player in args.Players)
        {
            var won = TryComp<WinningMarkerComponent>(player, out var winning) && winning.Challenge == uid;
            _stationWareChallenge.SetPlayerChallengeState(player, uid, won, args.Component);
            if (won && winning != null)
                RemComp(player, winning);
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
        if (HasComp<WinningMarkerComponent>(args.User))
            return;

        var winning = _componentFactory.GetComponent<WinningMarkerComponent>();
        winning.Challenge = component.Challenge;
        winning.Owner = args.User;
        EntityManager.AddComponent(args.User, winning);
        RemComp(uid, component);
    }
}
