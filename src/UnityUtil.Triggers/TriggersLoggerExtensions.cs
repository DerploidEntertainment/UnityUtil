using Microsoft.Extensions.Logging;

namespace UnityUtil.Triggers;

/// <inheritdoc/>
internal static class TriggersLoggerExtensions
{
    #region Information

    public static void RepeaterTriggerStarted(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerStarted)), "Started");

    public static void RepeaterTriggerStopped(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerStopped)), "Stopped");

    public static void RepeaterTriggerPaused(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerPaused)), "Paused");

    public static void RepeaterTriggerResumed(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerResumed)), "Resumed");

    public static void RepeaterTriggerTickForever(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerTickForever)), "Tick");

    public static void RepeaterTriggerTick(this ILogger logger, uint passedCount, uint targetCount) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerTick)), $"Tick {{{nameof(passedCount)}}} / {{{nameof(targetCount)}}}", passedCount, targetCount);

    public static void RepeaterTriggerTickCountReached(this ILogger logger, uint count) =>
        logger.LogInformation(new EventId(id: 0, nameof(RepeaterTriggerTickCountReached)), $"Reached {{{nameof(count)}}} ticks", count);

    public static void TimerTriggerStarted(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(TimerTriggerStarted)), "Started");

    public static void TimerTriggerStopped(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(TimerTriggerStopped)), "Stopped");

    public static void TimerTriggerPaused(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(TimerTriggerPaused)), "Paused");

    public static void TimerTriggerResumed(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(TimerTriggerResumed)), "Resumed");

    public static void TimerTriggerTimedOut(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(TimerTriggerTimedOut)), "Timed out");

    public static void ApplicationFocusChanged(this ILogger logger, bool isFocused) =>
        logger.LogInformation(new EventId(id: 0, nameof(ApplicationFocusChanged)), $"Application focus updated to {{{nameof(isFocused)}}}", isFocused);

    public static void ApplicationPauseChanged(this ILogger logger, bool isPaused) =>
        logger.LogInformation(new EventId(id: 0, nameof(ApplicationPauseChanged)), $"Application pause state updated to {{{nameof(isPaused)}}}", isPaused);

    public static void ApplicationQuitting(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ApplicationQuitting)), "Application quitting...");

    #endregion

    #region Warning

    public static void SequenceTriggerNullStep(this ILogger logger, int step) =>
        logger.LogWarning(new EventId(id: 0, nameof(SequenceTriggerNullStep)), $"Triggered at step {{{nameof(step)}}}, but the trigger was null", step);

    #endregion
}
