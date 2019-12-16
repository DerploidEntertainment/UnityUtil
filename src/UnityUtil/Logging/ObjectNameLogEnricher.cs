﻿namespace UnityEngine.Logging {

    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(UnityEngine.Logging) + "/" + nameof(ObjectNameLogEnricher), fileName = "object-name-log-enricher")]
    public class ObjectNameLogEnricher : LogEnricher {

        [Tooltip("'{0}' will be replaced by the name of the Object. Read more about .NET composite formatting here: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting")]
        public string FormatString = "{0}";
        [Tooltip("For Objects that are scene objects, the name will include up to this many parent objects' names in the logs. Use this to generate logs that more specifically identify a scene Object.")]
        public int NumParents = UnityObjectExtensions.DefaultNumParents;
        [Tooltip("For Objects that are scene objects, the name of the Object and its " + nameof(NumParents) + " parents will be separated by this string.")]
        public string AncestorNameSeparator = UnityObjectExtensions.DefaultAncestorSeparator;

        public override string GetEnrichedLog(object source) {
            var sourceObj = (source as Object);
            if (sourceObj == null)
                return string.Empty;

            var component = (source as Component);
            string name = (component == null) ? sourceObj.name : component.GetHierarchyName(NumParents, AncestorNameSeparator);
                return string.Format(FormatString, name);
        }
    }

}
