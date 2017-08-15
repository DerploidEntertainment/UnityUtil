using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Danware.Unity.Triggers {

    public class TimerTrigger : MonoBehaviour {

        // HIDDEN FIELDS
        private Coroutine _timerCoroutine;

        // INSPECTOR FIELDS
        [Tooltip("The time, in seconds, before the next (or first) Tick event.")]
        public float TimeBeforeTick = 1;
        [Tooltip("Should the timer Tick forever?  NumRepeats value will be ignored if this is set.")]
        public bool TickForever = true;
        [Tooltip("How many Ticks should be raised before stopping?  Ignored if RepeatForever is true.")]
        public int NumTicks = 1;
        [Tooltip("Should the Timer be restarted every time it is re-enabled?")]
        public bool StartOnEnable = false;

        public UnityEvent Starting = new UnityEvent();
        public UnityEvent Tick = new UnityEvent();
        public UnityEvent Stopped = new UnityEvent();
        public UnityEvent NumTicksReached = new UnityEvent();

        // EVENT HANDLERS
        private void OnEnable() {
            if (StartOnEnable)
                doStart();
        }
        private void OnDisable() {
            if (_timerCoroutine != null)
                doStop();
        }

        // API INTERFACE
        public void StartTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because it is disabled!");
            if (_timerCoroutine != null)
                return;

            doStart();
        }
        public void RestartTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because it is disabled!");

            if (_timerCoroutine != null)
                doStop();
            doStart();
        }
        public void StopTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {nameof(TimerTrigger)} {transform.parent.name}.{name} because it is disabled!");

            if (_timerCoroutine == null)
                return;

            doStop();
        }

        // HELPERS
        private void doStart() {
            Debug.Log($"{nameof(TimerTrigger)} {name} starting in frame {Time.frameCount}.");
            Starting.Invoke();

            _timerCoroutine = StartCoroutine(runTimer());
        }
        private void doStop() {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;

            Debug.Log($"{nameof(TimerTrigger)} {name} stopped in frame {Time.frameCount}.");
            Stopped.Invoke();
        }
        private void ticksReached() {
            Debug.Log($"{nameof(TimerTrigger)} {name} reached {NumTicks} ticks in frame {Time.frameCount}.");
            NumTicksReached.Invoke();

            if (_timerCoroutine != null)
                doStop();
        }
        private IEnumerator runTimer() {
            int numTicks = 1;
            do {
                yield return new WaitForSeconds(TimeBeforeTick);
                Debug.Log($"{nameof(TimerTrigger)} {name}: tick {numTicks} / {(TickForever ? "infinity" : NumTicks.ToString())}.");
                Tick.Invoke();
                ++numTicks;
            } while (TickForever || numTicks <= NumTicks);

            // If the desired number of ticks was reached, stop the Timer
            ticksReached();
        }


    }

}
