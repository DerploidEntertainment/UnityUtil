using UnityEngine;

namespace Serilog.Enrichers.Unity;

public class UnityLogEnricherSettings
{
    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.frameCount"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeFrameCount { get; set; } = true;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeSinceLevelLoad"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeTimeSinceLevelLoad { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeSinceLevelLoadAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeTimeSinceLevelLoadAsDouble { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.unscaledTime"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeUnscaledTime { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.unscaledTimeAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeUnscaledTimeAsDouble { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.time"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeTime { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with <see cref="Time.timeAsDouble"/>.
    /// See the Unity Scripting API docs for <a href="https://docs.unity3d.com/ScriptReference/Time.html"><c>Time</c></a>.
    /// </summary>
    public bool IncludeTimeAsDouble { get; set; } = false;

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with the hierarchy path of scene objects, if applicable.
    /// Use this to generate logs that more specifically identify a scene Object.
    /// </summary>
    public bool IncludeSceneObjectPath { get; set; } = true;

    /// <summary>
    /// If <see cref="IncludeSceneObjectPath"/> is <see langword="true"/> then,
    /// for Objects that are scene objects, the name will include up to this many parent objects' names in the logs.
    /// </summary>
    public int ParentCount { get; set; } = 1;

    /// <summary>
    /// If <see cref="IncludeSceneObjectPath"/> is <see langword="true"/> then,
    /// for Objects that are scene objects, the name of the Object and its <see cref="ParentCount"/> parents will be separated by this string.
    /// </summary>
    public string ParentNameSeparator { get; set; } = "/";
}
