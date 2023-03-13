namespace Content.Server._StationWare.Body;

[RegisterComponent]
public sealed class GibOnCollideComponent : Component
{
    [DataField("allowMultipleHits")]
    public bool AllowMultipleHits = true;
}
