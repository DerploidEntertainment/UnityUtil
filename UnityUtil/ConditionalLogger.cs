using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using U = UnityEngine;

namespace UnityUtil {

    public static class ConditionalLogger {

        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void Log(object message, bool framePrefix = true) => U.Debug.Log(getLog(message, framePrefix));
        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void LogWarning(object message, bool framePrefix = true) => U.Debug.LogWarning(getLog(message, framePrefix));
        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void LogError(object message, bool framePrefix = true) => U.Debug.LogError(getLog(message, framePrefix));

        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void Log<T>(this T component, object message, bool framePrefix = true, bool componentPrefix = true) where T : MonoBehaviour =>
            U.Debug.Log(getLog(component, message, framePrefix, componentPrefix));
        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void LogWarning<T>(this T component, object message, bool framePrefix = true, bool componentPrefix = true) where T : MonoBehaviour =>
            U.Debug.LogWarning(getLog(component, message, framePrefix, componentPrefix));
        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        public static void LogError<T>(this T component, object message, bool framePrefix = true, bool componentPrefix = true) where T : MonoBehaviour =>
            U.Debug.LogError(getLog(component, message, framePrefix, componentPrefix));

        public static string GetHierarchyName(this GameObject gameObject, int numParents = 1) => getName(gameObject.transform, numParents);
        public static string GetHierarchyName<T>(this T component, int numParents = 1) where T : MonoBehaviour => getName(component.transform, numParents);
        public static string GetHierarchyNameWithType<T>(this T component, int numParents = 1) where T : MonoBehaviour =>
            $"{component.GetType().Name} {getName(component.transform, numParents)}";

        public static void AssertDependency<T>(this T component, object member) where T : MonoBehaviour =>
            Assert.IsNotNull(member, $"{component.GetHierarchyNameWithType()}'s {nameof(member)} dependency was not satisfied!");
        public static string GetAssociationAssertion<T>(this T component, string memberName) where T : MonoBehaviour =>
            $"{component.GetHierarchyNameWithType()} was not associated with any {memberName}!";

        public static string GetSwitchDefault<T>(T value) => $"Gah!  We haven't accounted for {value.GetType().Name} {value} yet!";

        // HELPERS
        private static string getLog(object message, bool framePrefix) {
            string frameStr = framePrefix ? $"Frame {Time.frameCount}: " : string.Empty;
            return frameStr + message;
        }
        private static string getLog<T>(T component, object message, bool framePrefix, bool componentPrefix) where T : MonoBehaviour {
            string frameStr = framePrefix ? $"Frame {Time.frameCount}: " : string.Empty;
            string componentStr = componentPrefix ? $"{component.GetHierarchyNameWithType()}" : string.Empty;
            return frameStr + componentStr + message;
        }
        private static string getName(Transform transform, int numParents) {
            Transform trans = transform;
            string name = trans.name;
            for (int p = 0; p < numParents; ++p) {
                trans = trans.parent;
                name = ((trans == null) ? string.Empty : trans.name + ".") + name;
            }

            return "\"" + name + "\"";
        }


    }

}
