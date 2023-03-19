using Content.Server._StationWare.Challenges;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [Serializable]
    [DataDefinition]
    public sealed class SetChallengeState : IThresholdBehavior
    {
        [DataField("win")]
        public bool Win = true;

        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (cause == null)
                return;

            var entManager = system.EntityManager;
            var challengeSys = entManager.System<StationWareChallengeSystem>();
            var query = entManager.EntityQueryEnumerator<StationWareChallengeComponent>();
            while (query.MoveNext(out var uid, out var challenge))
            {
                challengeSys.SetPlayerChallengeState(cause.Value, uid, Win, challenge);
            }
        }
    }
}
