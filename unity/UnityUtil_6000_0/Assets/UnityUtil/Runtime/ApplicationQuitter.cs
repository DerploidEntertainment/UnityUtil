using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace UnityUtil
{
    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(ApplicationQuitter), fileName = "application-quitter")]
    public class ApplicationQuitter : ScriptableObject, IApplicationQuitter
    {
        [Button]
        public void Quit(int exitCode = 0)
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit(exitCode);
#endif
        }

    }
}
