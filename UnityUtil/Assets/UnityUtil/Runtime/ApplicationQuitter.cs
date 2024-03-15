using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace UnityUtil
{
    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(ApplicationQuitter), fileName = "application-quitter")]
    public class ApplicationQuitter : ScriptableObject
    {
        [Button]
        public void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

    }
}
