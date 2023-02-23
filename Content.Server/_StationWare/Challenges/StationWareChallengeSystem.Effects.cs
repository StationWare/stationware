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
        foreach (var challengeEffect in EntityQuery<ChallengeStateEffectComponent>())
        {
            if (challengeEffect.Challenge != ev.Challenge)
                continue;
            var ent = challengeEffect.Owner;
            EnsureComp<TimedDespawnComponent>(ent).Lifetime = challengeEffect.DisappearDelay;
        }
    }
}


