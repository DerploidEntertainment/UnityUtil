using UnityEngine;

namespace Serilog.Enrichers.UnityObjectPath;

[CreateAssetMenu(menuName = $"{nameof(Serilog)}/Enrichers/{nameof(UnityObjectPathLogEnricherSettings)}", fileName = "unity-object-path-log-enricher-settings")]
public class UnityObjectPathLogEnricherSettings : ScriptableObject
{
    [Tooltip(
        "For Objects that are scene objects, the name will include up to this many parent objects' names in the logs. " +
        "Use this to generate logs that more specifically identify a scene Object."
    )]
    public uint NumParents = 1u;

    [Tooltip($"For Objects that are scene objects, the name of the Object and its {nameof(NumParents)} parents will be separated by this string.")]
    public string AncestorNameSeparator = "/";
}
