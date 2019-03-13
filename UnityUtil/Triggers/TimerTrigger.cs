using System;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    [Serializable]
    public class CountEvent : UnityEvent<uint> { }

    public class TimerTrigger : Updatable {

        // HIDDEN FIELDS
        private bool _running = false;

        // INSPECTOR FIELDS
        [Tooltip("The time, in seconds, before the next (or first) " + nameof(TimerTrigger.Tick) + " event.")]
        public float TimeBeforeTick = 1f;
        [Tooltip("The time, in seconds, that has passed since the previous " + nameof(TimerTrigger.Tick) + " event.")]
        public float TimeSincePreviousTick = 0f;
        [Tooltip("How many " + nameof(TimerTrigger.Tick) + "s should be raised before stopping?  Ignored if " + nameof(TimerTrigger.TickForever) + " is true.")]
        public uint NumTicks = 1u;
        [Tooltip("If true, then this " + nameof(Triggers.TimerTrigger) + " will raise " + nameof(TimerTrigger.Tick) + " events forever.  If false, then it will only " + nameof(TimerTrigger.Tick) + " " + nameof(TimerTrigger.NumTicks) + " times.")]
        public bool TickForever = false;
        [Tooltip("Number of " + nameof(TimerTrigger.Tick) + " events that have already been raised.")]
        public uint NumPassedTicks = 0u;
        [Tooltip("Should this " + nameof(Triggers.TimerTrigger) + " be restarted every time it is re-enabled?")]
        public bool StartOnEnable = false;
        public bool Logging = true;

        public UnityEvent Starting = new UnityEvent();
        public CountEvent Tick = new CountEvent();
        public UnityEvent Stopped = new UnityEvent();
        public UnityEvent NumTicksReached = new UnityEvent();

        // EVENT HANDLERS
        protected override void BetterAwake() => RegisterUpdatesAutomatically = false;
        protected override void BetterOnEnable() {
            if (StartOnEnable)
                doStart();
        }
        protected override void BetterOnDisable() {
            if (_running)
                doStop();
        }

        // API INTERFACE
        public void StartTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot start {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot start {this.GetHierarchyNameWithType()} because it is disabled!");
            if (_running)
                return;

            doStart();
        }
        public void RestartTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot restart {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot restart {this.GetHierarchyNameWithType()} because it is disabled!");

            if (_running)
                doStop();
            doStart();
        }
        public void PauseTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot pause {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot pause {this.GetHierarchyNameWithType()} because it is disabled!");

            if (!_running)
                return;

            _running = false;
            Updater.UnregisterUpdate(InstanceID);
            if (Logging)
                this.Log($" paused.");
        }
        public void ResumeTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot resume {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot resume {this.GetHierarchyNameWithType()} because it is disabled!");

            if (_running)
                return;

            _running = true;
            Updater.RegisterUpdate(InstanceID, updateTimer);
            if (Logging)
                this.Log(" resumed.");
        }
        public void StopTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {this.GetHierarchyNameWithType()} because it is disabled!");

            if (!_running)
                return;

            doStop();
        }

        // HELPERS
        private void doStart() {
            if (Logging)
                this.Log(" starting.");
            Starting.Invoke();

            Updater.RegisterUpdate(InstanceID, updateTimer);

            TimeSincePreviousTick = 0f;
            NumPassedTicks = 0u;
            _running = true;
        }
        private void doStop() {
            _running = false;
            TimeSincePreviousTick = 0f;

            Updater.UnregisterUpdate(InstanceID);

            if (Logging)
                this.Log($" stopped.");
            Stopped.Invoke();
        }
        private void updateTimer() {
            // Update the time elapsed, if the Timer is running
            if (!_running)
                return;
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
            if (_running)   // May now be false if any UnityEvents manually stopped this timer
                doStop();
        }


    }

}
