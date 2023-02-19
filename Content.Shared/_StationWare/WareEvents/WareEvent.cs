using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._StationWare.WareEvents;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract class WareEvent
{
    /// <summary>
    /// How many players out of the pool should
    /// be selected for this event?
    /// </summary>
    [DataField("playersToAffect")]
    public int? PlayersToAffect;

    /// <summary>
    /// Function called when the WareEvent begins.
    /// </summary>
    /// <param name="players">A list of valid players for the event</param>
    /// <param name="entity"></param>
    /// <param name="random"></param>
    public abstract void Run(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random);

    /// <summary>
    /// Function called when the WareEvent ends, before the next one begins.
    /// </summary>
    /// <param name="players">A list of valid players for the event</param>
    /// <param name="entity"></param>
    /// <param name="random"></param>
    public virtual void End(IEnumerable<EntityUid> players, IEntityManager entity, IRobustRandom random)
    {

    }
}
