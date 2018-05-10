using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityUtil.Triggers {

    [Serializable]
    public class CountEvent : UnityEvent<uint> { }

    public class TimerTrigger : Updatable {

        // HIDDEN FIELDS
        private bool _running = false;

        // INSPECTOR FIELDS
        [Tooltip("The time, in seconds, before the next (or first) " + nameof(TimerTrigger.Tick) + " event.")]
        public float TimeBeforeTick = 1f;
        [Tooltip("The time, in seconds, that has passed since the previous Tick event.")]
        public float TimeSincePreviousTick = 0f;
        [Tooltip("How many " + nameof(TimerTrigger.Tick) + "s should be raised before stopping?  Set to 'Infinity' to tick forever.")]
        public uint NumTicks = 1u;
        [Tooltip("Number of " + nameof(TimerTrigger.Tick) + " eventss that have already been raised.")]
        public uint NumPassedTicks = 0u;
        [Tooltip("Should this " + nameof(Triggers.TimerTrigger) + " be restarted every time it is re-enabled?")]
        public bool StartOnEnable = false;

        public UnityEvent Starting = new UnityEvent();
        public CountEvent Tick = new CountEvent();
        public UnityEvent Stopped = new UnityEvent();
        public UnityEvent NumTicksReached = new UnityEvent();

        // EVENT HANDLERS
        protected override void BetterAwake() {
            BetterUpdate = updateTimer;
            RegisterUpdatesAutomatically = true;
        }
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
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {this.GetHierarchyNameWithType()} because it is disabled!");
            if (_running)
                return;

            doStart();
        }
        public void RestartTimer() {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot stop {this.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(enabled, $"Cannot stop {this.GetHierarchyNameWithType()} because it is disabled!");

            if (_running)
                doStop();
            doStart();
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
            this.Log(" starting.");
            Starting.Invoke();

            TimeSincePreviousTick = 0f;
            NumPassedTicks = 0u;
            _running = true;
        }
        private void doStop() {
            _running = false;
            TimeSincePreviousTick = 0f;

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
                this.Log($": tick {NumPassedTicks} / {NumTicks}");
                Tick.Invoke(NumPassedTicks);
                TimeSincePreviousTick = 0f;
                ++NumPassedTicks;
            }
            if (NumPassedTicks < NumTicks)
                return;

            // If the desired number of ticks was reached, then stop the Timer
            this.Log($" reached {NumTicks} ticks.");
            NumTicksReached.Invoke();
            doStop();
        }


    }

}
