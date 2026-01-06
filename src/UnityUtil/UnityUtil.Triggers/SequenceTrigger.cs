using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Triggers;

public class SequenceTrigger : MonoBehaviour
{
    private ILogger<SequenceTrigger>? _logger;

    [Tooltip($"The current step; i.e., the index (0-based) of {nameof(StepTriggers)} that will be invoked the next time {nameof(Trigger)} is called.")]
    public int CurrentStep = 0;
    [Tooltip(
        $"If true, then {nameof(CurrentStep)} will wrap around whenever it goes past the first or last index of {nameof(StepTriggers)}. " +
        $"If false, then calls to {nameof(StepByOne)} will be clamped between the first and last index."
    )]
    public bool Cycle;
    [Tooltip(
        $"The sequence of triggers to iterate through. Every time {nameof(StepByOne)} is called, the {nameof(CurrentStep)} index will be incremented. " +
        $"Call {nameof(Trigger)} to invoke the trigger at {nameof(CurrentStep)} (multiple times, if desired)."
    )]
    public UnityEvent[] StepTriggers = [];

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    [PropertySpace]

    [Button]
    public void Trigger()
    {
        if (StepTriggers[CurrentStep] is null) {
            log_NullStep(CurrentStep);
            return;
        }

        StepTriggers[CurrentStep]?.Invoke();
    }

    [PropertySpace]

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StepByOne() => doStep(stepDelta: 1, thenTrigger: false);

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StepByOneAndTrigger() => doStep(stepDelta: 1, thenTrigger: true);

    [PropertySpace]

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStep(int step) => doStep(step - CurrentStep, thenTrigger: false);

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStepAndTrigger(int step) => doStep(step - CurrentStep, thenTrigger: true);

    [PropertySpace]

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StepByCount(int stepCount) => doStep(stepCount, thenTrigger: false);

    [Button]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StepByCountAndTrigger(int stepCount) => doStep(stepCount, thenTrigger: true);

    private void doStep(int stepDelta, bool thenTrigger)
    {
        int newStep = Cycle
            ? (CurrentStep + StepTriggers.Length + stepDelta % StepTriggers.Length) % StepTriggers.Length
            : Mathf.Clamp(CurrentStep + stepDelta, 0, StepTriggers.Length - 1);

        CurrentStep = newStep;

        if (thenTrigger)
            Trigger();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, int, Exception?> LOG_NULL_STEP_ACTION =
        LoggerMessage.Define<int>(Warning, new EventId(id: 0, nameof(log_NullStep)), "Triggered at step {Step}, but the trigger was null");
    private void log_NullStep(int step) => LOG_NULL_STEP_ACTION(_logger!, step, null);

    #endregion
}

