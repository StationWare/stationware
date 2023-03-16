using System.Linq;
using Content.Shared._StationWare.Points;
using Robust.Shared.GameStates;

namespace Content.Client._StationWare.Points;

public sealed class PointSystem : SharedPointSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointManagerComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, PointManagerComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not PointManagerComponentState state)
            return;
        component.Points = new(state.Points);
    }

    // TODO: this is kinda awful but i don't want to rewrite the API
    public PointManagerComponent? GetPointManagerClient()
    {
        var query = EntityQuery<PointManagerComponent>().ToList();
        if (!query.Any())
        {
            return null;
        }
        return query.First();
    }
}
