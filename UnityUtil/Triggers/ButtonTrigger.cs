﻿using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ButtonTrigger : Updatable {

        private float _tRefactory = -1f;

        // INSPECTOR FIELDS
        public UnityEvent Triggered = new UnityEvent();
        public bool CanPress = true;
        [Tooltip("Time, in seconds, before the button may be pressed again.")]
        public float RefactoryPeriod = 1f;

        protected override void BetterAwake() => RegisterUpdatesAutomatically = false;
        protected override void BetterOnEnable() {
            if (_tRefactory > -1f)
                Updater.RegisterUpdate(InstanceID, updateRefactory);
        }
        protected override void BetterOnDisable() {
            if (_tRefactory > -1f)
                Updater.UnregisterUpdate(InstanceID);
        }

        // API INTERFACE
        [Button]
        public void Press() {
            // Don't press the button if its still in the refractory period
            if (!CanPress)
                return;

            // Otherwise, raise the trigger event and prevent the button from being pressed for the desired period
            Triggered.Invoke();
            CanPress = false;
            _tRefactory = 0f;
            Updater.RegisterUpdate(InstanceID, updateRefactory);
        }

        // HIDDEN FUNCTIONS
        private void updateRefactory(float deltaTime) {
            if (_tRefactory <= RefactoryPeriod) {
                _tRefactory += deltaTime;
                return;
            }

            _tRefactory = 0f;
            Updater.UnregisterUpdate(InstanceID);

            CanPress = true;
        }

    }

}
