namespace Content.Server._StationWare.Physics;

[RegisterComponent]
public sealed class BouncyComponent : Component
{
    [DataField("minLinearVelocity")]
    public float MinLinearVelocity = 1.5f;

    [DataField("maxLinearVelocity")]
    public float MaxLinearVelocity = 1.5f;

    [DataField("angularVelocity")]
    public float AngularVelocity = 12f;
}
