using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.Logging;

namespace UnityEngine.Triggers {

    public class SequenceTrigger : MonoBehaviour {

        private ILogger _logger;

        [Tooltip("The current step; i.e., the index (0-based) of " + nameof(StepTriggers) + " that will be invoked the next time " + nameof(Trigger) + " is called..")]
        public int CurrentStep = 0;
        [Tooltip("If true, then " + nameof(CurrentStep) + " will wrap around whenever it goes past the first or last index of " + nameof(StepTriggers) + ". If false, then calls to " + nameof(Step) + " will be clamped between the first and last index.")]
        public bool Cycle;
        [Tooltip("The sequence of triggers to iterate through. Every time " + nameof(Step) + " is called, the " + nameof(CurrentStep) + " index will be incremented. Call " + nameof(Trigger) + " to invoke the trigger at " + nameof(CurrentStep) + " (multiple times, if desired).")]
        public UnityEvent[] StepTriggers;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        private void Awake() => DependencyInjector.ResolveDependenciesOf(this);

        [Button]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StepAndTrigger() => doStep(1, thenTrigger: true);
        [Button]
        public void StepAndTrigger(int step) => doStep(step, thenTrigger: true);

        [Button]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Step() => doStep(1, thenTrigger: false);
        [Button]
        public void Step(int step) => doStep(step, thenTrigger: false);

        [Button]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trigger() {
            if (StepTriggers[CurrentStep] == null)
                _logger.LogWarning($"Triggered at step {CurrentStep}, but the trigger was null!", context: this);
            else
                StepTriggers[CurrentStep]?.Invoke();
        }

        private void doStep(int step, bool thenTrigger) {
            int newStep = Cycle
                ? (CurrentStep + StepTriggers.Length + (step % StepTriggers.Length)) % StepTriggers.Length
                : Mathf.Clamp(CurrentStep + step, 0, StepTriggers.Length - 1);

            CurrentStep = newStep;

            if (thenTrigger)
                Trigger();
        }

    }

}

