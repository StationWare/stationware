using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._StationWare.Challenges;

/// <summary>
/// This is a prototype for a StationWare challenge.
/// </summary>
[Prototype("challenge")]
public sealed class ChallengePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Whether or not the players win automatically or
    /// lose automatically.
    /// </summary>
    [DataField("winByDefault")]
    public readonly bool WinByDefault;

    /// <summary>
    /// Tags for categorizing/filtering challenges
    /// </summary>
    [DataField("tags", customTypeSerializer: typeof(PrototypeIdListSerializer<TagPrototype>))]
    public readonly List<string> Tags = new();

    /// <summary>
    /// How long the challenge lasts.
    /// </summary>
    [DataField("duration")]
    public readonly TimeSpan? Duration;

    /// <summary>
    /// A delay between the challenge announcement
    /// and the actual challenge beginning.
    /// </summary>
    [DataField("startDelay")]
    public readonly TimeSpan StartDelay = TimeSpan.Zero;

    /// <summary>
    /// The announcement played when the event starts
    /// </summary>
    [DataField("announcement")]
    public readonly string Announcement = default!;

    /// <summary>
    /// The sound played when the event starts
    /// Defaults to the funny ding.
    /// </summary>
    [DataField("announcementSound")]
    public readonly SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/_StationWare/event_ding.ogg");

    /// <summary>
    /// Components that are added to the challenge entity
    /// to dictate specific behaviors and conditions about
    /// the challenge itself.
    /// </summary>
    [DataField("challengeModifiers")]
    public readonly EntityPrototype.ComponentRegistry ChallengeModifiers = new();
}
