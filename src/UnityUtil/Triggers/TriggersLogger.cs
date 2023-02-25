using Microsoft.Extensions.Logging;
using UnityUtil.Logging;

namespace UnityUtil.Triggers;

/// <inheritdoc/>
internal class TriggersLogger<T> : BaseUnityUtilLogger<T>
{
    public TriggersLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 13_000) { }

    #region Information

    public void RepeaterTriggerStarted() =>
        LogInformation(id: 0, nameof(RepeaterTriggerStarted), "Started");

    public void RepeaterTriggerStopped() =>
        LogInformation(id: 1, nameof(RepeaterTriggerStopped), "Stopped");

    public void RepeaterTriggerPaused() =>
        LogInformation(id: 2, nameof(RepeaterTriggerPaused), "Paused");

    public void RepeaterTriggerResumed() =>
        LogInformation(id: 3, nameof(RepeaterTriggerResumed), "Resumed");

    public void RepeaterTriggerTickForever() =>
        LogInformation(id: 4, nameof(RepeaterTriggerTickForever), "Tick");

    public void RepeaterTriggerTick(uint passedCount, uint targetCount) =>
        LogInformation(id: 5, nameof(RepeaterTriggerTick), $"Tick {{{nameof(passedCount)}}} / {{{nameof(targetCount)}}}");

    public void RepeaterTriggerTickCountReached(uint count) =>
        LogInformation(id: 6, nameof(RepeaterTriggerTickCountReached), $"Reached {{{nameof(count)}}} ticks", count);

    public void TimerTriggerStarted() =>
        LogInformation(id: 7, nameof(TimerTriggerStarted), "Started");

    public void TimerTriggerStopped() =>
        LogInformation(id: 8, nameof(TimerTriggerStopped), "Stopped");

    public void TimerTriggerPaused() =>
        LogInformation(id: 9, nameof(TimerTriggerPaused), "Paused");

    public void TimerTriggerResumed() =>
        LogInformation(id: 10, nameof(TimerTriggerResumed), "Resumed");

    public void TimerTriggerTimedOut() =>
        LogInformation(id: 11, nameof(TimerTriggerTimedOut), "Timed out");

    public void ApplicationFocusChanged(bool isFocused) =>
        LogInformation(id: 12, nameof(ApplicationFocusChanged), $"Application focus updated to {{{nameof(isFocused)}}}", isFocused);

    public void ApplicationPauseChanged(bool isPaused) =>
        LogInformation(id: 13, nameof(ApplicationPauseChanged), $"Application pause state updated to {{{nameof(isPaused)}}}", isPaused);

    public void ApplicationQuitting() =>
        LogInformation(id: 14, nameof(ApplicationQuitting), "Application quitting...");

    #endregion

    #region Warning

    public void SequenceTriggerNullStep(int step) =>
        LogWarning(id: 0, nameof(SequenceTriggerNullStep), $"Triggered at step {{{nameof(step)}}}, but the trigger was null", step);

    #endregion
}
