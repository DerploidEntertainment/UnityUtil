using Sirenix.OdinInspector;
using System;
using UnityEngine.Serialization;

namespace UnityEngine.Triggers {

    public enum EnableDisableBehavior {
        PauseResume,
        StopRestart,
        StopRestartAlways,
    }

    public abstract class StartStoppable : Updatable {

        private bool _starting = true;
        private bool _wasRunningB4Disable = false;

        // INSPECTOR FIELDS
        [Tooltip("What should happen when this component is enabled/disabled? " + nameof(EnableDisableBehavior.PauseResume) + " will pause/resume it, if it was running. " + nameof(EnableDisableBehavior.StopRestart) + " will stop/restart it, if it was running. " + nameof(EnableDisableBehavior.StopRestartAlways) + " will stop/restart it, restarting it even if it was not previously running. In all cases, the first OnEnable is still controlled by " + nameof(StartAutomatically) + ".")]
        public EnableDisableBehavior EnableDisableBehavior = EnableDisableBehavior.PauseResume;
        [Tooltip("Should the repeater start automatically when this GameObject starts?")]
        [FormerlySerializedAs("StartOnEnable")]
        public bool StartAutomatically = false;

        // EVENT HANDLERS
        protected override void BetterAwake() => RegisterUpdatesAutomatically = false;
        protected override void BetterOnEnable() {
            // If the GameObject is starting (i.e., this is the first-ever call to OnEnable)...
            if (_starting) {
                _starting = false;
                if (StartAutomatically)
                    DoRestart();
            }

            // Otherwise...
            else {
                switch (EnableDisableBehavior) {
                    case EnableDisableBehavior.PauseResume:
                        if (_wasRunningB4Disable)
                            DoResume();
                        break;

                    case EnableDisableBehavior.StopRestart:
                        if (_wasRunningB4Disable)
                            DoRestart();
                        break;

                    case EnableDisableBehavior.StopRestartAlways:
                        DoRestart();
                        break;

                    default:
                        throw new NotImplementedException(BetterLogger.GetSwitchDefault(StartAutomatically));
                }
            }
        }
        protected override void BetterOnDisable() {
            _wasRunningB4Disable = Running;

            switch (EnableDisableBehavior) {
                case EnableDisableBehavior.PauseResume:
                    DoPause();
                    break;

                case EnableDisableBehavior.StopRestart:
                case EnableDisableBehavior.StopRestartAlways:
                    DoStop();
                    break;

                default:
                    throw new NotImplementedException(BetterLogger.GetSwitchDefault(StartAutomatically));
            }
        }

        // API INTERFACE
        public bool Running { get; private set; } = false;

        [Button("Start")]
        public void StartBehavior() {
            AssertAcitveAndEnabled("start");
            if (Running)
                return;

            DoRestart();
        }
        [Button("Restart")]
        public void RestartBehavior() {
            AssertAcitveAndEnabled("restart");
            if (Running)
                DoStop();
            DoRestart();
        }
        [Button("Pause")]
        public void PauseBehavior() {
            AssertAcitveAndEnabled("pause");
            DoPause();
        }
        [Button("Resume")]
        public void ResumeBehavior() {
            AssertAcitveAndEnabled("resume");
            DoResume();
        }
        [Button("Stop")]
        public void StopBehavior() {
            AssertAcitveAndEnabled("stop");
            if (!Running)
                return;

            DoStop();
        }

        // HELPERS
        protected virtual void DoRestart() {
            Updater.RegisterUpdate(InstanceID, DoUpdate);
            Running = true;
        }
        protected virtual void DoStop() {
            if (!Running)
                return;

            Running = false;
            Updater.UnregisterUpdate(InstanceID);
        }
        protected virtual void DoPause() {
            if (!Running)
                return;

            Running = false;
            Updater.UnregisterUpdate(InstanceID);
        }
        protected virtual void DoResume() {
            if (Running)
                return;

            Running = true;
            Updater.RegisterUpdate(InstanceID, DoUpdate);
        }
        protected abstract void DoUpdate();

    }

}
