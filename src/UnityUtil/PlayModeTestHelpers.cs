using UnityEngine.SceneManagement;

namespace UnityUtil {

    public static class PlayModeTestHelpers {

        public static void ResetScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

}
