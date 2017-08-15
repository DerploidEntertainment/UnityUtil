using Danware.Unity.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Danware.Unity {

    public class GameStateManagerSingleton : MonoBehaviour {

        // HIDDEN FIELDS

        // INSPECTOR INTERFACE
        [Tooltip("The input to use to toggle the paused state of the game.  If not set, then the game can only be paused programmatically.")]
        public StartStopInput TogglePauseInput;
        public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;
        public UnityEvent Paused = new UnityEvent();
        public UnityEvent Unpaused = new UnityEvent();

        // API INTERFACE
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) {
            // Adjust the paused state
            bool old = IsPaused;
            IsPaused = paused;
            Time.timeScale = IsPaused ? 0f : 1f;

            // Raise the corresponding event
            Debug.Log($"Game {(IsPaused ? "paused" : "resumed")} in frame {Time.frameCount}");
            (IsPaused ? Paused : Unpaused).Invoke();
        }
        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode);
        public void LoadSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode);
        public void UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);
        public void RestartScene() {
            // Unpause and reload the active Scene
            SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void Quit() => Application.Quit();

        private void Update() {
            if (TogglePauseInput?.Started() ?? false)
                SetPaused(!IsPaused);
        }

    }

}
