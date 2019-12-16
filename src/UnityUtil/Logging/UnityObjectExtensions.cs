using System;
using System.Diagnostics;
using System.Text;
using UnityEngine.Assertions;

namespace UnityEngine.Logging {

    public static class UnityObjectExtensions {

        public const int DefaultNumParents = 1;
        public const string DefaultAncestorSeparator = ">";

        public static string GetHierarchyName(this GameObject gameObject, int numParents = DefaultNumParents, string separator = DefaultAncestorSeparator) => GetName(gameObject.transform, numParents, separator);
        public static string GetHierarchyName(this Component component, int numParents = DefaultNumParents, string separator = DefaultAncestorSeparator) => GetName(component.transform, numParents, separator);
        public static string GetHierarchyNameWithType(this Component component, int numParents = DefaultNumParents, string separator = DefaultAncestorSeparator) =>
            $"{component.GetType().Name} {GetName(component.transform, numParents, separator)}";

        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertDependency(this Component component, object member, string memberName) =>
            Assert.IsNotNull(member, $"{component.GetHierarchyNameWithType()}'s {memberName} dependency was not satisfied!");
        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertAssociation(this Component component, object member, string memberName) =>
            Assert.IsNotNull(member, $"{component.GetHierarchyNameWithType()} was not associated with any {memberName}!");
        /// <summary>
        /// Assert that this component is both active and enabled.
        /// </summary>
        /// <param name="verbMessage">
        /// If this component is either inactive or disabled, then this verb will be used in the logged error message.
        /// Should be present-tense phrase, like "stop", or "perform that action". Padding spaces are not required.
        /// </param>
        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertAcitveAndEnabled(this Behaviour behaviour, string verbMessage = "use") {
            Assert.IsTrue(behaviour.gameObject.activeInHierarchy, $"Cannot {verbMessage} {behaviour.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(behaviour.enabled, $"Cannot {verbMessage} {behaviour.GetHierarchyNameWithType()} because it is disabled!");
        }

        public static string GetSwitchDefault<T>(T value) where T : Enum => $"Gah! We haven't accounted for {value.GetType().Name} {value} yet!";

        // HELPERS
        private static string GetName(Transform transform, int numParents, string separator) {
            Transform trans = transform;
            var nameBuilder = new StringBuilder(trans.name);
            for (int p = 0; p < numParents; ++p) {
                trans = trans.parent;
                if (trans == null)
                    break;
                nameBuilder.Insert(0, trans.name + separator);
            }

            return $"'{nameBuilder}'";
        }
    }
}
