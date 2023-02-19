using Content.Server._StationWare.WareEvents;
using Content.Server.GameTicking.Rules;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._StationWare.StationEvents;

/// <summary>
/// runs wareevents at regular intervals.
/// </summary>
public sealed class WareEventSchedulerSystem : GameRuleSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    public override void Initialize()
    {
        base.Initialize();

        _configuration.OnValueChanged(CCVars.StationWareWareEventInterval, v=> _eventInterval = v, true);
    }

    public override string Prototype => "WareEventScheduler";

    [Dependency] private readonly WareEventSystem _wareEvent = default!;

    private float _eventInterval = 60;
    private const float MinimumTimeUntilFirstEvent = 10;

    private float _timeUntilNextEvent = MinimumTimeUntilFirstEvent;

    public override void Started() { }

    public override void Ended()
    {
        _timeUntilNextEvent = MinimumTimeUntilFirstEvent;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!RuleStarted)
            return;

        if (_timeUntilNextEvent > 0)
        {
            _timeUntilNextEvent -= frameTime;
            return;
        }

        _wareEvent.RunWareEvent();
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
