using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Input;

namespace UnityEngine {

    [DisallowMultipleComponent]
    public class GameStateManager : MonoBehaviour {

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
            this.Log($" {(IsPaused ? "paused" : "resumed")} the game.");
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

        // EVENT HANDLERS
        private void Update() {
            if (TogglePauseInput?.Started() ?? false)
                SetPaused(!IsPaused);
        }

    }

}
