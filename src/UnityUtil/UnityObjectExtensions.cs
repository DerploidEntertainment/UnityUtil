﻿using System;
using System.Diagnostics;
using System.Text;
using UnityEngine.Assertions;

namespace UnityEngine {

    public static class UnityObjectExtensions {

        public const uint DefaultNumParents = 1u;
        public const string DefaultAncestorSeparator = ">";
        public const string DefaultHierarchyNameFormatString = "'{0}'";
        public const string DefaultHierarchyNameWithTypeFormatString = "{0} '{1}'";

        public static string GetHierarchyName(
            this GameObject gameObject,
            uint numParents = DefaultNumParents,
            string separator = DefaultAncestorSeparator,
            string formatString = DefaultHierarchyNameFormatString
        ) => getName(gameObject.transform, numParents, separator, formatString);
        public static string GetHierarchyName(
            this Component component,
            uint numParents = DefaultNumParents,
            string separator = DefaultAncestorSeparator,
            string formatString = DefaultHierarchyNameFormatString
        ) => getName(component.transform, numParents, separator, formatString);
        public static string GetHierarchyNameWithType(
            this Component component,
            uint numParents = DefaultNumParents,
            string separator = DefaultAncestorSeparator,
            string formatString = DefaultHierarchyNameWithTypeFormatString
        ) => string.Format(formatString, component.GetType().Name, getName(component.transform, numParents, separator, formatString: "{0}"));

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
        public static void AssertActiveAndEnabled(this Behaviour behaviour, string verbMessage = "use") {
            Assert.IsTrue(behaviour.gameObject.activeInHierarchy, $"Cannot {verbMessage} {behaviour.GetHierarchyNameWithType()} because its GameObject is inactive!");
            Assert.IsTrue(behaviour.enabled, $"Cannot {verbMessage} {behaviour.GetHierarchyNameWithType()} because it is disabled!");
        }

        public static NotImplementedException SwitchDefaultException<T>(T value) where T : Enum =>
            new NotImplementedException($"Gah! We haven't accounted for {typeof(T).Name} {value} yet!");

        // HELPERS
        private static string getName(Transform transform, uint numParents, string separator, string formatString) {
            Transform trans = transform;
            var nameBuilder = new StringBuilder(trans.name);
            for (int p = 0; p < numParents; ++p) {
                trans = trans.parent;
                if (trans == null)
                    break;
                nameBuilder.Insert(0, trans.name + separator);
            }

            return string.Format(formatString, nameBuilder);
        }
    }
}
