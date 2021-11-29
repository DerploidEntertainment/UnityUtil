using UnityEngine.Events;
using UnityEngine.Logging;

namespace UnityEngine.Triggers {

    public class TimerTrigger : StartStoppable
    {
        [Tooltip("The duration, in seconds, before the " + nameof(TimerTrigger.Timeout) + " event.")]
        public float Duration = 1f;
        [Tooltip("The time, in seconds, that has passed since the timer started.")]
        public float TimePassed = 0f;
        public bool Logging = false;

        public UnityEvent Timeout = new();
        public UnityEvent Stopped = new();

        public float PercentProgress => TimePassed / Duration;

        protected override void DoRestart() {
            base.DoRestart();

            if (Logging)
                Logger.Log("Starting", context: this);

            TimePassed = 0f;
        }
        protected override void DoStop() {
            base.DoStop();

            if (Logging)
                Logger.Log($"Stopped", context: this);
            Stopped.Invoke();
        }
        protected override void DoPause() {
            base.DoStop();

            if (Logging)
                Logger.Log($"Paused", context: this);
        }
        protected override void DoResume() {
            base.DoResume();

            if (Logging)
                Logger.Log("Resumed", context: this);
        }
        protected override void DoUpdate(float deltaTime) {
            // Update the time elapsed, if the Timer is running
            TimePassed += deltaTime;
            if (TimePassed <= Duration)
                return;

            // Once the timer is up, raise the Timeout event
            if (Logging)
                Logger.Log("Timed out!", context: this);
            Timeout.Invoke();
            if (Running)   // May now be false if any UnityEvents manually stopped this timer
                DoStop();
        }


    }

}
