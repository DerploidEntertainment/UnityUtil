using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Triggers;

[Serializable]
public class CountEvent : UnityEvent<uint> { }

public class RepeaterTrigger : StartStoppable
{
    private ILogger<RepeaterTrigger>? _logger;

    [Tooltip($"The time, in seconds, before the next (or first) {nameof(Tick)} event.")]
    public float TimeBeforeTick = 1f;

    [Tooltip($"The time, in seconds, that has passed since the previous {nameof(Tick)} event.")]
    public float TimeSincePreviousTick = 0f;

    [Tooltip($"How many {nameof(Tick)}s should be raised before stopping? Ignored if {nameof(TickForever)} is true.")]
    public uint NumTicks = 1u;

    [Tooltip(
        $"If true, then this {nameof(RepeaterTrigger)} will raise {nameof(Tick)} events forever. " +
        $"If false, then it will only {nameof(Tick)} {nameof(NumTicks)} times."
    )]
    public bool TickForever = false;

    [Tooltip($"Number of {nameof(Tick)} events that have already been raised.")]
    public uint NumPassedTicks = 0u;

    public bool Logging;

    public CountEvent Tick = new();
    public UnityEvent Stopped = new();
    public UnityEvent NumTicksReached = new();

    public float PercentProgress => TimeSincePreviousTick / TimeBeforeTick;
    public float PercentTickProgress => NumPassedTicks / NumTicks;

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    protected override void DoRestart()
    {
        base.DoRestart();

        if (Logging)
            log_Started();

        TimeSincePreviousTick = 0f;
        NumPassedTicks = 0u;
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
        TimeSincePreviousTick += deltaTime;

        // If another Tick period has passed, then raise the Tick event
        if (TimeSincePreviousTick >= TimeBeforeTick) {
            if (Logging) {
                if (TickForever)
                    log_TickForever();
                else
                    log_Tick(NumPassedTicks, NumTicks);
            }
            Tick.Invoke(NumPassedTicks);
            TimeSincePreviousTick = 0f;
            ++NumPassedTicks;
        }
        if (NumPassedTicks < NumTicks || TickForever)
            return;

        // If the desired number of ticks was reached, then stop the Timer
        if (Logging)
            log_TickCountReached(NumTicks);
        NumTicksReached.Invoke();
        if (Running)   // May now be false if any UnityEvents manually stopped this repeater
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


    private static readonly Action<MEL.ILogger, Exception?> LOG_TICK_FOREVER_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_TickForever)), "Tick");
    private void log_TickForever() => LOG_TICK_FOREVER_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, uint, uint, Exception?> LOG_TICK_ACTION =
        LoggerMessage.Define<uint, uint>(Information, new EventId(id: 0, nameof(log_Tick)), "Tick {PassedCount} / {TargetCount}");
    private void log_Tick(uint passedCount, uint targetCount) => LOG_TICK_ACTION(_logger!, passedCount, targetCount, null);


    private static readonly Action<MEL.ILogger, uint, Exception?> LOG_TICK_COUNT_REACHED_ACTION =
        LoggerMessage.Define<uint>(Information, new EventId(id: 0, nameof(log_TickCountReached)), "Reached {TargetCount} ticks");
    private void log_TickCountReached(uint targetCount) => LOG_TICK_COUNT_REACHED_ACTION(_logger!, targetCount, null);

    #endregion
}
