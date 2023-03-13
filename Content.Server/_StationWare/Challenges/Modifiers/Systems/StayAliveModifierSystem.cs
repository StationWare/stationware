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
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        var enumerator = EntityQueryEnumerator<StayAliveModifierComponent, StationWareChallengeComponent>();
        while (enumerator.MoveNext(out var uid, out _, out var challenge))
        {
            if (_mobState.IsIncapacitated(ev.Target, ev.Component))
                _stationWareChallenge.SetPlayerChallengeState(ev.Target, uid, false, challenge);
        }
    }
}
