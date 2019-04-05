﻿using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.Triggers {

    public class TimerTrigger : StartStoppable {

        // INSPECTOR FIELDS
        [Tooltip("The duration, in seconds, before the " + nameof(TimerTrigger.Timeout) + " event.")]
        [FormerlySerializedAs("TimeBeforeTick")]
        public float Duration = 1f;
        [Tooltip("The time, in seconds, that has passed since the timer started.")]
        public float TimePassed = 0f;
        public bool Logging = false;

        public UnityEvent Starting = new UnityEvent();
        [FormerlySerializedAs("Tick")]
        public UnityEvent Timeout = new UnityEvent();
        public UnityEvent Stopped = new UnityEvent();

        // API INTERFACE
        public float PercentProgress => TimePassed / Duration;

        // HELPERS
        protected override void DoRestart() {
            base.DoRestart();

            if (Logging)
                this.Log(" starting.");
            Starting.Invoke();

            TimePassed = 0f;
        }
        protected override void DoStop() {
            base.DoStop();

            TimePassed = 0f;

            if (Logging)
                this.Log($" stopped.");
            Stopped.Invoke();
        }
        protected override void DoPause() {
            base.DoStop();

            if (Logging)
                this.Log($" paused.");
        }
        protected override void DoResume() {
            base.DoResume();

            if (Logging)
                this.Log(" resumed.");
        }
        protected override void DoUpdate() {
            // Update the time elapsed, if the Timer is running
            TimePassed += UnityEngine.Time.deltaTime;
            if (TimePassed <= Duration)
                return;

            // Once the timer is up, raise the Timeout event
            if (Logging)
                this.Log(" timed out!");
            Timeout.Invoke();
            if (Running)   // May now be false if any UnityEvents manually stopped this timer
                DoStop();
        }


    }

}
