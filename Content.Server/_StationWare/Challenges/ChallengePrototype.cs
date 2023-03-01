using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

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
    /// A list of prototypes to quickly query/get a list of prototypes.
    /// </summary>
    [DataField("tags", customTypeSerializer: typeof(ListSerializers<PrototypeIdSerializer<TagPrototype>>))]
    public List<string> Tags = default!;

    /// <summary>
    /// Whether or not the players win automatically or
    /// lose automatically.
    /// </summary>
    [DataField("winByDefault")]
    public bool WinByDefault;

    /// <summary>
    /// How long the challenge lasts.
    /// </summary>
    [DataField("duration")]
    public TimeSpan? Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// A delay between the challenge announcement
    /// and the actual challenge beginning.
    /// </summary>
    [DataField("startDelay")]
    public TimeSpan StartDelay = TimeSpan.Zero;

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
    /// Components that are added to the challenge entity
    /// to dictate specific behaviors and conditions about
    /// the challenge itself.
    /// </summary>
    [DataField("challengeModifiers")]
    public EntityPrototype.ComponentRegistry ChallengeModifiers = new();
}
