using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._StationWare.WareEvents;

[Prototype("wareEvent")]
public sealed class WareEventPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// The announcement played when the event starts
    /// </summary>
    [DataField("announcement")]
    public string Announcement = default!;

    /// <summary>
    /// The sound played when the event starts
    /// Defaults to the funny ding.
    /// </summary>
    [DataField("announcementSound")]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/_StationWare/event_ding.ogg");

    /// <summary>
    /// The event itself that is ran. Can contain extra datafields for defining extra behaviors.
    /// </summary>
    [DataField("event", required: true, serverOnly: true)]
    public WareEvent Event = default!;
}
