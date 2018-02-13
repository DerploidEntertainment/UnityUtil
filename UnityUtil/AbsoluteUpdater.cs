using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    public abstract class AbsoluteUpdater : MonoBehaviour {

        // HIDDEN FIELDS
        private float _lastRealtime;
        protected float _delta;

        // INSPECTOR FIELDS
        public GameStateManager GameStateManager;
        public bool PauseWhenGameIsPaused = true;

        private void Awake() {
            Assert.IsNotNull(GameStateManager, this.GetAssociationAssertion(nameof(this.GameStateManager)));

            _lastRealtime = Time.realtimeSinceStartup;
        }
        private void Update() {
            float t = Time.realtimeSinceStartup;
            _delta = t - _lastRealtime;
            _lastRealtime = t;

            // It is possible that the calculated delta time is less than zero,
            // especially if this script is attached to an object that is created when the scene is loaded
            // In that case, discard this update.
            if (_delta < 0) {
                ConditionalLogger.LogWarning($"Delta time was negative ({_delta})...discarding.");
                _delta = 0;
            }

            if (PauseWhenGameIsPaused && GameStateManager.IsPaused)
                _delta = 0;

            doUpdates();
        }

        protected abstract void doUpdates();
        protected IEnumerator waitForAbsoluteSeconds(float seconds) {
            float elapsedTime = 0;
            while (elapsedTime < seconds) {
                yield return null;
                elapsedTime += _delta;
            }
        }

    }

}