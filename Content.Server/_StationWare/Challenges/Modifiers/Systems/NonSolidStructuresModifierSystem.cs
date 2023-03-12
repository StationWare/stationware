using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed class NonSolidStructuresModifierSystem : EntitySystem
{
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NonSolidStructuresModifierComponent, ChallengeStartEvent>(OnChallengeStart);
        SubscribeLocalEvent<NonSolidStructuresModifierComponent, ChallengeEndEvent>(OnChallengeEnd);
    }

    private void OnChallengeStart(EntityUid uid, NonSolidStructuresModifierComponent component, ref ChallengeStartEvent args)
    {
        var enumerator = EntityQueryEnumerator<TagComponent, FixturesComponent>();
        while (enumerator.MoveNext(out var ent, out var tag, out var fixtures))
        {
            if (!_tag.HasTag(tag, component.TargetTag))
                continue;

            foreach (var fixture in fixtures.Fixtures.Values)
            {
                if (!fixture.Hard)
                    continue;
                _physics.SetHard(ent, fixture, false, fixtures);
                component.AffectedFixtures.Add(fixture);
            }
        }
    }

    private void OnChallengeEnd(EntityUid uid, NonSolidStructuresModifierComponent component, ref ChallengeEndEvent args)
    {
        foreach (var fixture in component.AffectedFixtures)
        {
            _physics.SetHard(fixture.Body.Owner, fixture, true);
        }
        component.AffectedFixtures.Clear();
    }
}
