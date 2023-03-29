namespace Content.Server._StationWare.Challenges.Modifiers.Components;

[RegisterComponent]
public sealed class MathModifierComponent : Component
{
    [DataField("minRange")] public int MinimumRange = 1;
    [DataField("maxRange")] public int MaximumRange = 10;
    public float Answer = 0;
}

[RegisterComponent]
public sealed class MathPlayerComponent : Component
{
    [DataField("challenge")] public EntityUid Challenge;
}

