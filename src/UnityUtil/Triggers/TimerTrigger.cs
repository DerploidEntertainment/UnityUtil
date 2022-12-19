using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

public class TimerTrigger : StartStoppable
{
    private TriggersLogger<TimerTrigger>? _logger;

    [Tooltip($"The duration, in seconds, before the {nameof(Timeout)} event.")]
    public float Duration = 1f;
    [Tooltip("The time, in seconds, that has passed since the timer started.")]
    public float TimePassed = 0f;
    public bool Logging;

    public UnityEvent Timeout = new();
    public UnityEvent Stopped = new();

    public float PercentProgress => TimePassed / Duration;

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

    protected override void DoRestart()
    {
        base.DoRestart();

        if (Logging)
            _logger!.TimerTriggerStarted();

        TimePassed = 0f;
    }
    protected override void DoStop()
    {
        base.DoStop();

        if (Logging)
            _logger!.TimerTriggerStopped();
        Stopped.Invoke();
    }
    protected override void DoPause()
    {
        base.DoStop();

        if (Logging)
            _logger!.TimerTriggerPaused();
    }
    protected override void DoResume()
    {
        base.DoResume();

        if (Logging)
            _logger!.TimerTriggerResumed();
    }
    protected override void DoUpdate(float deltaTime)
    {
        // Update the time elapsed, if the Timer is running
        TimePassed += deltaTime;
        if (TimePassed <= Duration)
            return;

        // Once the timer is up, raise the Timeout event
        if (Logging)
            _logger!.TimerTriggerTimedOut();
        Timeout.Invoke();
        if (Running)   // May now be false if any UnityEvents manually stopped this timer
            DoStop();
    }


}
