using System.Collections;
using UnityEngine.Logging;

namespace UnityEngine {

    public abstract class AbsoluteUpdater : MonoBehaviour {

        // HIDDEN FIELDS
        private ILogger _logger;
        private float _lastRealtime;
        protected float _delta;

        // INSPECTOR FIELDS
        public GameStateManager GameStateManager;
        public bool PauseWhenGameIsPaused = true;

        public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        private void Awake() {
            DependencyInjector.ResolveDependenciesOf(this);

            this.AssertAssociation(GameStateManager, nameof(GameStateManager));

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
                _logger.LogWarning($"Delta time was negative ({_delta})...discarding.", context: this);
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
