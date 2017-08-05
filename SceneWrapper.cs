using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Danware.Unity.Input;

namespace Danware.Unity {

    public class SceneWrapper : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class SceneEventArgs : EventArgs {
            public SceneEventArgs(Scene scene) {
                Scene = scene;
            }
            public Scene Scene { get; }
        }
        [Serializable]
        public class SceneEvent : UnityEvent<SceneEventArgs> { }

        // HIDDEN FIELDS
        private bool _paused;

        // INSPECTOR INTERFACE
        public StartStopInput PauseInput;
        public SceneEvent Paused;
        public SceneEvent Resumed;
        public SceneEvent SceneRestarted;

        // API INTERFACE
        public bool IsPaused {
            get => _paused;
            set => resetPaused(value);
        }
        public void RestartScene() {
            // Unpause and reload the active Scene
            resetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            // Raise the Scene restarted event
            var args = new SceneEventArgs(SceneManager.GetActiveScene());
            SceneRestarted.Invoke(args);
        }
        public void Quit() => Application.Quit();

        // EVENT HANDLERS
        private void Update() {
            // Handle player input
            if (PauseInput.Started())
                resetPaused(!_paused);
        }

        // HELPERS
        private void resetPaused(bool paused) {
            // Adjust the paused state
            bool old = _paused;
            _paused = paused;

            // Pause/unpause the game by adjusting its time scale
            Time.timeScale = _paused ? 0f : 1f;

            // Raise the corresponding event, if a change actually occurred
            if (_paused != old) {
                var args = new SceneEventArgs(SceneManager.GetActiveScene());
                SceneEvent e = _paused ? Paused : Resumed;
                e.Invoke(args);
            }
        }

    }

}
