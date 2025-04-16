using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Triggers;

public class TimerTrigger : StartStoppable
{
    private ILogger<TimerTrigger>? _logger;

    [Tooltip($"The duration, in seconds, before the {nameof(Timeout)} event.")]
    public float Duration = 1f;
    [Tooltip("The time, in seconds, that has passed since the timer started.")]
    public float TimePassed = 0f;
    public bool Logging;

    public UnityEvent Timeout = new();
    public UnityEvent Stopped = new();

    public float PercentProgress => TimePassed / Duration;

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    protected override void DoRestart()
    {
        base.DoRestart();

        if (Logging)
            log_Started();

        TimePassed = 0f;
    }
    protected override void DoStop()
    {
        base.DoStop();

        if (Logging)
            log_Stopped();
        Stopped.Invoke();
    }
    protected override void DoPause()
    {
        base.DoStop();

        if (Logging)
            log_Paused();
    }
    protected override void DoResume()
    {
        base.DoResume();

        if (Logging)
            log_Resumed();
    }
    protected override void DoUpdate(float deltaTime)
    {
        // Update the time elapsed, if the Timer is running
        TimePassed += deltaTime;
        if (TimePassed <= Duration)
            return;

        // Once the timer is up, raise the Timeout event
        if (Logging)
            log_TimedOut();
        Timeout.Invoke();
        if (Running)   // May now be false if any UnityEvents manually stopped this timer
            DoStop();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_STARTED_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Started)), "Started");
    private void log_Started() => LOG_STARTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_STOPPED_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Stopped)), "Stopped");
    private void log_Stopped() => LOG_STOPPED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_PAUSED_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Paused)), "Paused");
    private void log_Paused() => LOG_PAUSED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_RESUMED_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Resumed)), "Resumed");
    private void log_Resumed() => LOG_RESUMED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_TIMED_OUT_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_TimedOut)), "Timed out");
    private void log_TimedOut() => LOG_TIMED_OUT_ACTION(_logger!, null);

    #endregion
}
