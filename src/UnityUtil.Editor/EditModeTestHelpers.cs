using UnityEditor.SceneManagement;

namespace UnityUtil.Editor {

    public static class EditModeTestHelpers {

        public static void ResetScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

    }

}
