using UnityEngine;

namespace UnityUtil.Logging;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(Logging)}/{nameof(ObjectNameLogEnricher)}", fileName = "object-name-log-enricher")]
public class ObjectNameLogEnrichSettings : ScriptableObject
{
    [Tooltip(
        "For Objects that are scene objects, the name will include up to this many parent objects' names in the logs. " +
        "Use this to generate logs that more specifically identify a scene Object."
    )]
    public uint NumParents = UnityObjectExtensions.DefaultNumParents;

    [Tooltip($"For Objects that are scene objects, the name of the Object and its {nameof(NumParents)} parents will be separated by this string.")]
    public string AncestorNameSeparator = UnityObjectExtensions.DefaultAncestorSeparator;

    [Tooltip(
        "'{0}' will be replaced by the name of the Object. " +
        "Read more about .NET composite formatting here: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"
    )]
    public string FormatString = UnityObjectExtensions.DefaultHierarchyNameFormatString;
}
