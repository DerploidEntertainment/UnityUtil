using UnityEngine.Events;
using UnityEngine.Inputs;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityEngine {

    [DisallowMultipleComponent]
    public class GameStateManager : MonoBehaviour {

        private ILogger _logger;

        // INSPECTOR INTERFACE
        [Tooltip("The input to use to toggle the paused state of the game.  If not set, then the game can only be paused programmatically.")]
        public StartStopInput TogglePauseInput;
        public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;
        public UnityEvent Paused = new UnityEvent();
        public UnityEvent Unpaused = new UnityEvent();

        private void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);
        private void Awake() => DependencyInjector.ResolveDependenciesOf(this);

        // API INTERFACE
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) {
            // Adjust the paused state
            IsPaused = paused;
            Time.timeScale = IsPaused ? 0f : 1f;

            // Raise the corresponding event
            _logger.Log($" {(IsPaused ? "paused" : "resumed")} the game.");
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
        public void SetCursorConfined() => Cursor.lockState = CursorLockMode.Confined;
        public void SetCursorLocked() => Cursor.lockState = CursorLockMode.Locked;
        public void SetCursorUnlocked() => Cursor.lockState = CursorLockMode.None;

        // EVENT HANDLERS
        private void Update() {
            if (TogglePauseInput?.Started() ?? false)
                SetPaused(!IsPaused);
        }

    }

}
