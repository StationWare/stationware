using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents;

namespace Content.Server._StationWare.StationEvents;

/// <summary>
/// runs events at regular intervals.
/// </summary>
public sealed class RegularStationEventSchedulerSystem : GameRuleSystem
{
    public override string Prototype => "RegularStationEventScheduler";

    [Dependency] private readonly EventManagerSystem _event = default!;

    [DataField("eventInterval"), ViewVariables(VVAccess.ReadWrite)]
    private float _eventInterval = 60;

    private const float MinimumTimeUntilFirstEvent = 60;

    private float _timeUntilNextEvent = MinimumTimeUntilFirstEvent;

    public override void Started() { }

    public override void Ended()
    {
        _timeUntilNextEvent = MinimumTimeUntilFirstEvent;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!RuleStarted || !_event.EventsEnabled)
            return;

        if (_timeUntilNextEvent > 0)
        {
            _timeUntilNextEvent -= frameTime;
            return;
        }

        _event.RunRandomEvent();
        ResetTimer();
    }

    /// <summary>
    /// Reset the event timer once the event is done.
    /// </summary>
    private void ResetTimer()
    {
        _timeUntilNextEvent = _eventInterval;
    }
}
