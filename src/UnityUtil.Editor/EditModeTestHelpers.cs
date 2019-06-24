using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityUtil.Editor {

    public static class EditModeTestHelpers {

        public static void ResetScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

    }

}
