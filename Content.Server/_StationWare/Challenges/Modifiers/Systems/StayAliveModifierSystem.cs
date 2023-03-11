using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class StayAliveModifierSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StationWareChallengeSystem _stationWareChallenge = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StayAliveModifierComponent, BeforeChallengeEndEvent>(OnBeforeChallengeEnd);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnBeforeChallengeEnd(EntityUid uid, StayAliveModifierComponent component, ref BeforeChallengeEndEvent args)
    {
        if (!TryComp<StationWareChallengeComponent>(uid, out var challenge))
            return;

        foreach (var player in args.Players)
        {
            _stationWareChallenge.SetPlayerChallengeState(player, uid, _mobState.IsAlive(player), challenge);
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        foreach (var (stayAlive, challenge) in EntityQuery<StayAliveModifierComponent, StationWareChallengeComponent>())
        {
            if (_mobState.IsIncapacitated(ev.Target, ev.Component))
                _stationWareChallenge.SetPlayerChallengeState(ev.Target, stayAlive.Owner, false, challenge);
        }
    }
}
