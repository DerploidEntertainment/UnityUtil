using Sirenix.OdinInspector;
using UnityEngine;

namespace Serilog.Enrichers.Unity;

[CreateAssetMenu(menuName = $"{nameof(Serilog)}/Enrichers/{nameof(UnityLogEnricherSettings)}", fileName = "unity-log-enricher-settings")]
public class UnityLogEnricherSettings : ScriptableObject
{
    private const string GRP_TIME = "Include Time";

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.frameCount` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html."
    )]
    [LabelText("frameCount")]
    public bool IncludeFrameCount = true;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.timeSinceLevelLoad` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html."
    )]
    [LabelText("timeSinceLevelLoad")]
    public bool IncludeTimeSinceLevelLoad = false;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.timeSinceLevelLoadAsDouble` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html."
    )]
    [LabelText("timeSinceLevelLoadAsDouble")]
    public bool IncludeTimeSinceLevelLoadAsDouble = false;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.unscaledTime` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html. " +
        "Note that these Unity timestamps are fairly redundant with the log timestamp already provided by Serilog."
    )]
    [LabelText("unscaledTime")]
    public bool IncludeUnscaledTime = false;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.unscaledTimeAsDouble` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html. " +
        "Note that these Unity timestamps are fairly redundant with the log timestamp already provided by Serilog."
    )]
    [LabelText("unscaledTimeAsDouble")]
    public bool IncludeUnscaledTimeAsDouble = false;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.time` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html."
    )]
    [LabelText("time")]
    public bool IncludeTime = false;

    [BoxGroup(GRP_TIME)]
    [Tooltip(
        "If true, then every log is encriched with the Unity `Time.timeAsDouble` value. " +
        "See `Time` in the Unity Scripting API docs: https://docs.unity3d.com/ScriptReference/Time.html."
    )]
    [LabelText("timeAsDouble")]
    public bool IncludeTimeAsDouble = false;

    [ToggleGroup(nameof(IncludeSceneObjectPath))]
    [Tooltip("Enable this to generate logs that more specifically identify a scene Object.")]
    public bool IncludeSceneObjectPath = true;

    [Tooltip(
        "For Objects that are scene objects, the name will include up to this many parent objects' names in the logs. " +
        "Use this to generate logs that more specifically identify a scene Object."
    )]
    [ToggleGroup(nameof(IncludeSceneObjectPath))]
    public uint ParentCount = 1u;

    [Tooltip($"For Objects that are scene objects, the name of the Object and its {nameof(ParentCount)} parents will be separated by this string.")]
    [ToggleGroup(nameof(IncludeSceneObjectPath))]
    public string ParentNameSeparator = "/";
}
