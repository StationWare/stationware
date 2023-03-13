using Content.Shared.Spawners.Components;

namespace Content.Server._StationWare.Challenges;

public sealed partial class StationWareChallengeSystem
{
    private void InitializeEffects()
    {
        SubscribeLocalEvent<ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeEnd(ref ChallengeEndEvent ev)
    {
        var enumerator = EntityQueryEnumerator<ChallengeStateEffectComponent>();
        while (enumerator.MoveNext(out var ent, out var challengeEffect))
        {
            if (challengeEffect.Challenge != ev.Challenge)
                continue;
            EnsureComp<TimedDespawnComponent>(ent).Lifetime = challengeEffect.DisappearDelay;
        }
    }
}


