using Content.Server._StationWare.Challenges.Modifiers.Components;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class AddComponentModifierSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<AddComponentModifierComponent, ChallengeStartEvent>(OnChallengeStart);
    }

    private void OnChallengeStart(EntityUid uid, AddComponentModifierComponent component, ref ChallengeStartEvent args)
    {
        // copy all our modifiers from the challenge component to the entity
        foreach (var (name, entry) in component.AddedComponents)
        {
            var reg = _componentFactory.GetRegistration(name);
            var comp = (Component) _componentFactory.GetComponent(reg);
            comp.Owner = uid;
            var temp = (object) comp;
            _serialization.CopyTo(entry.Component, ref temp);
            EntityManager.AddComponent(uid, (Component) temp!, true);
            component.ProvidedComponents.Add(comp);
        }
    }

    private void OnChallengeEnd(EntityUid uid, AddComponentModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var comp in component.ProvidedComponents)
        {
            _entityManager.RemoveComponent(comp.Owner, comp);
        }
    }
}
