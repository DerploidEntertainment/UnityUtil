using UnityEngine.SceneManagement;

namespace UnityEngine
{

    [DisallowMultipleComponent]
    public class GameStateManager : Configurable {

        // INSPECTOR INTERFACE
        public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;

        // API INTERFACE
        #pragma warning disable CA1822 // Mark members as static
        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode);
        public void LoadSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode);
        public void UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);
        public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        public void SetCursorConfined() => Cursor.lockState = CursorLockMode.Confined;
        public void SetCursorLocked() => Cursor.lockState = CursorLockMode.Locked;
        public void SetCursorUnlocked() => Cursor.lockState = CursorLockMode.None;
        #pragma warning restore CA1822 // Mark members as static

    }

}
