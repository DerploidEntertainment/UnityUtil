using Serilog.Events;
using UE = UnityEngine;

namespace Serilog.Sinks.Unity;

/// <summary>
/// Settings for a <see cref="UnitySink"/>.
/// </summary>
public class UnitySinkSettings
{
    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> storing the Unity log's tag, if present.
    /// If <see langword="null"/>, then <see cref="UnitySink"/> will not check the <see cref="LogEvent.Properties"/> for a tag.
    /// This property can be removed from <see cref="LogEvent"/>s by setting <see cref="RemoveUnityTagLogPropertyIfPresent"/> to <see langword="true"/>
    /// to avoid duplicating its value in the log message (recommended).
    /// See Unity's <a href="https://docs.unity3d.com/ScriptReference/Logger.Log.html"><c>Logger.Log</c></a> docs for a description of the <c>tag</c> parameter.
    /// </summary>
    public string? UnityTagLogProperty { get; set; } = "UnityLogTag";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> storing the Unity log's context, if present.
    /// If <see langword="null"/>, then <see cref="UnitySink"/> will not check the <see cref="LogEvent.Properties"/> for a context.
    /// This property can be removed from <see cref="LogEvent"/>s by setting <see cref="RemoveUnityContextLogPropertyIfPresent"/> to <see langword="true"/>
    /// to avoid trying to format <see cref="UE.Object"/> instances stored in the context property (recommended).
    /// See Unity's <a href="https://docs.unity3d.com/ScriptReference/Debug.Log.html"><c>Debug.Log</c></a> docs for a description of the <c>context</c> parameter.
    /// </summary>
    public string? UnityContextLogProperty { get; set; } = "UnityLogContext";

    /// <summary>
    /// Whether the <see cref="LogEventProperty"/> storing the Unity log's tag (the property with the name given by <see cref="UnityTagLogProperty"/>)
    /// will be removed from <see cref="LogEvent"/>s.
    /// Set to <see langword="true"/> to avoid duplicating that property's value between the log's tag in the Unity Console and the formatted log message itself.
    /// See Unity's <a href="https://docs.unity3d.com/ScriptReference/Logger.Log.html"><c>Logger.Log</c></a> docs for a description of the <c>tag</c> parameter.
    /// </summary>
    public bool RemoveUnityTagLogPropertyIfPresent { get; set; } = true;

    /// <summary>
    /// Whether the <see cref="LogEventProperty"/> storing the Unity log's context (the property with the name given by <see cref="UnityContextLogProperty"/>)
    /// will be removed from <see cref="LogEvent"/>s.
    /// Set to <see langword="true"/> to avoid trying to format <see cref="UE.Object"/> instances stored in the context property.
    /// See Unity's <a href="https://docs.unity3d.com/ScriptReference/Debug.Log.html"><c>Debug.Log</c></a> docs for a description of the <c>context</c> parameter.
    /// </summary>
    public bool RemoveUnityContextLogPropertyIfPresent { get; set; } = true;
}
