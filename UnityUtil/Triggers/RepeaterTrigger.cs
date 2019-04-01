using System;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    [Serializable]
    public class CountEvent : UnityEvent<uint> { }

    public class RepeaterTrigger : StartStoppable {

        // INSPECTOR FIELDS
        [Tooltip("The time, in seconds, before the next (or first) " + nameof(RepeaterTrigger.Tick) + " event.")]
        public float TimeBeforeTick = 1f;
        [Tooltip("The time, in seconds, that has passed since the previous " + nameof(RepeaterTrigger.Tick) + " event.")]
        public float TimeSincePreviousTick = 0f;
        [Tooltip("How many " + nameof(RepeaterTrigger.Tick) + "s should be raised before stopping?  Ignored if " + nameof(RepeaterTrigger.TickForever) + " is true.")]
        public uint NumTicks = 1u;
        [Tooltip("If true, then this " + nameof(Triggers.RepeaterTrigger) + " will raise " + nameof(RepeaterTrigger.Tick) + " events forever.  If false, then it will only " + nameof(RepeaterTrigger.Tick) + " " + nameof(RepeaterTrigger.NumTicks) + " times.")]
        public bool TickForever = false;
        [Tooltip("Number of " + nameof(RepeaterTrigger.Tick) + " events that have already been raised.")]
        public uint NumPassedTicks = 0u;
        public bool Logging = false;

        public UnityEvent Starting = new UnityEvent();
        public CountEvent Tick = new CountEvent();
        public UnityEvent Stopped = new UnityEvent();
        public UnityEvent NumTicksReached = new UnityEvent();

        // API INTERFACE
        public float PercentProgress => TimeSincePreviousTick / TimeBeforeTick;

        // HELPERS
        protected override void DoRestart() {
            base.DoRestart();

            if (Logging)
                this.Log(" starting.");
            Starting.Invoke();

            TimeSincePreviousTick = 0f;
            NumPassedTicks = 0u;
        }
        protected override void DoStop() {
            base.DoStop();

            TimeSincePreviousTick = 0f;

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
            TimeSincePreviousTick += Time.deltaTime;

            // If another Tick period has passed, then raise the Tick event
            if (TimeSincePreviousTick >= TimeBeforeTick) {
                if (Logging)
                    this.Log(TickForever ? ": tick!" : $": tick {NumPassedTicks} / {NumTicks}");
                Tick.Invoke(NumPassedTicks);
                TimeSincePreviousTick = 0f;
                ++NumPassedTicks;
            }
            if (NumPassedTicks < NumTicks || TickForever)
                return;

            // If the desired number of ticks was reached, then stop the Timer
            if (Logging)
                this.Log($" reached {NumTicks} ticks.");
            NumTicksReached.Invoke();
            if (Running)   // May now be false if any UnityEvents manually stopped this repeater
                DoStop();
        }


    }

}
