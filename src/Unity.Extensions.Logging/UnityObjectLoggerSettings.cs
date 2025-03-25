using Serilog.Events;
using UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// Settings for a <see cref="UnityObjectLogger{T}"/> instance.
/// </summary>
public class UnityObjectLoggerSettings
{
    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that holds the logging <see cref="Object"/> instance
    /// for use as <c>context</c> by Unity's <see cref="Debug.Log(object, Object)"/> method.
    /// If <see langword="null"/>, then no <c>context</c> will be added to log messages.
    /// </summary>
    public string? UnityContextLogProperty { get; set; } = "UnityLogContext";

    /// <summary>
    /// Name of the <see cref="LogEventProperty"/> that holds the logging <see cref="Object"/> instance's hierarchical name.
    /// If <see langword="null"/>, then no hierarchical name is calculated or added to the log message.
    /// <para>
    /// </para>
    /// For <see cref="GameObject"/> and <see cref="Component"/>-derived instances, the hierarchical name is
    /// the name of the object's transform and all parent transforms, separated by <see cref="ParentNameSeparator"/>.
    /// For all other <see cref="Object"/> instances, the hierarchical name is simply the object's <see cref="Object.name"/>.
    /// </summary>
    public string? HierarchicalNameLogProperty { get; set; } = "UnityHierarchyName";

    /// <summary>
    /// Whether the logging <see cref="Object"/> instance's hierarchy is static.
    /// I.e., for <see cref="GameObject"/> and <see cref="Component"/>-derived instances, whether its parent transforms ever change.
    /// If an object's hierarchy is static, then its hierarchical name is cached for better logging performance.
    /// </summary>
    public bool HasStaticHierarchy { get; set; } = true;

    /// <summary>
    /// For logging <see cref="Object"/>s that are <see cref="GameObject"/>s or derived from <see cref="Component"/>,
    /// the object's transform and all of its parent transforms will be separated by this string.
    /// Prefer single-character separators to keep rendered log messages small.
    /// </summary>
    public string ParentNameSeparator { get; set; } = ">";  // Better default than '/', so the hierarchical name isn't mistaken for a file path
}
