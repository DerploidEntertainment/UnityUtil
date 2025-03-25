using Serilog.Events;
using UnityEngine;

namespace Serilog.Enrichers.Unity;

/// <summary>
/// Settings for a <see cref="UnityLogEnricher"/>.
/// </summary>
public class UnityLogEnricherSettings
{
    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.frameCount"/>,
    /// if <see cref="AddFrameCount"/> is <see langword="true"/>.
    /// </summary>
    public string FrameCountLogProperty { get; set; } = "UnityFrameCount";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.timeSinceLevelLoad"/>,
    /// if <see cref="AddTimeSinceLevelLoad"/> is <see langword="true"/>.
    /// </summary>
    public string TimeSinceLevelLoadLogProperty { get; set; } = "UnityTimeSinceLevelLoad";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.timeSinceLevelLoadAsDouble"/>,
    /// if <see cref="AddTimeSinceLevelLoadAsDouble"/> is <see langword="true"/>.
    /// </summary>
    public string TimeSinceLevelLoadAsDoubleLogProperty { get; set; } = "UnityTimeSinceLevelLoadAsDouble";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.unscaledTime"/>,
    /// if <see cref="AddUnscaledTime"/> is <see langword="true"/>.
    /// </summary>
    public string UnscaledTimeLogProperty { get; set; } = "UnityUnscaledTime";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.unscaledTimeAsDouble"/>,
    /// if <see cref="AddUnscaledTimeAsDouble"/> is <see langword="true"/>.
    /// </summary>
    public string UnscaledTimeAsDoubleLogProperty { get; set; } = "UnityUnscaledTimeAsDouble";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.time"/>,
    /// if <see cref="AddTime"/> is <see langword="true"/>.
    /// </summary>
    public string TimeLogProperty { get; set; } = "UnityTime";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that will hold the <see cref="Time.timeAsDouble"/>,
    /// if <see cref="AddTimeAsDouble"/> is <see langword="true"/>.
    /// </summary>
    public string TimeAsDoubleLogProperty { get; set; } = "UnityTimeAsDouble";

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.frameCount"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddFrameCount { get; set; } = true;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeSinceLevelLoad"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddTimeSinceLevelLoad { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeSinceLevelLoadAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddTimeSinceLevelLoadAsDouble { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.unscaledTime"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddUnscaledTime { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.unscaledTimeAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddUnscaledTimeAsDouble { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.time"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddTime { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool AddTimeAsDouble { get; set; } = false;
}
