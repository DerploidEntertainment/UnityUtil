using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityUtil {

    public static class PlayModeTestHelpers {

        public static void ResetScene() {
            Transform[] transforms = Object.FindObjectsOfType<Transform>();
            for (int t = 0; t < transforms.Length; ++t)
                Object.Destroy(transforms[t].gameObject);
        }

    }

}
