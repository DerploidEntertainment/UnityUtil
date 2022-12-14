using Microsoft.Extensions.Logging;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Triggers;

/// <inheritdoc/>
internal class TriggersLogger<T> : BaseUnityUtilLogger<T>
{
    public TriggersLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 13_000) { }

    #region Trace

    #endregion

    #region Debug

    #endregion

    #region Information

    public void RepeaterTriggerStarted() =>
        Log(id: 0, nameof(RepeaterTriggerStarted), Information, "Started");

    public void RepeaterTriggerStopped() =>
        Log(id: 1, nameof(RepeaterTriggerStopped), Information, "Stopped");

    public void RepeaterTriggerPaused() =>
        Log(id: 2, nameof(RepeaterTriggerPaused), Information, "Paused");

    public void RepeaterTriggerResumed() =>
        Log(id: 3, nameof(RepeaterTriggerResumed), Information, "Resumed");

    public void RepeaterTriggerTickForever() =>
        Log(id: 4, nameof(RepeaterTriggerTickForever), Information, "Tick");

    public void RepeaterTriggerTick(uint passedCount, uint targetCount) =>
        Log(id: 5, nameof(RepeaterTriggerTick), Information, $"Tick {{{nameof(passedCount)}}} / {{{nameof(targetCount)}}}");

    public void RepeaterTriggerTickCountReached(uint count) =>
        Log(id: 6, nameof(RepeaterTriggerTickCountReached), Information, $"Reached {{{nameof(count)}}} ticks", count);

    public void TimerTriggerStarted() =>
        Log(id: 7, nameof(TimerTriggerStarted), Information, "Started");

    public void TimerTriggerStopped() =>
        Log(id: 8, nameof(TimerTriggerStopped), Information, "Stopped");

    public void TimerTriggerPaused() =>
        Log(id: 9, nameof(TimerTriggerPaused), Information, "Paused");

    public void TimerTriggerResumed() =>
        Log(id: 10, nameof(TimerTriggerResumed), Information, "Resumed");

    public void TimerTriggerTimedOut() =>
        Log(id: 11, nameof(TimerTriggerTimedOut), Information, "Timed out");

    #endregion

    #region Warning

    public void SequenceTriggerNullStep(int step) =>
        Log(id: 0, nameof(SequenceTriggerNullStep), Warning, $"Triggered at step {{{nameof(step)}}}, but the trigger was null", step);

    #endregion

    #region Error

    #endregion

    #region Critical

    #endregion
}
