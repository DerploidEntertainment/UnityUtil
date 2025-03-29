using UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// Settings for a <see cref="UnityObjectLogger{T}"/> instance.
/// </summary>
public class UnityObjectLoggerSettings
{

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with the logging <see cref="Object"/> instance.
    /// </summary>
    /// <remarks>
    /// This instance will be used as <c>context</c> for Unity's <see cref="Debug.Log(object, Object)"/> method.
    /// </remarks>
    public bool EnrichWithUnityContext { get; set; } = true;

    /// <summary>
    /// Name of the log property that holds the logging <see cref="Object"/> instance.
    /// </summary>
    /// <remarks><inheritdoc cref="EnrichWithUnityContext" path="/remarks"/></remarks>
    public string? UnityContextLogProperty { get; set; } = "UnityLogContext";

    /// <summary>
    /// If <see langword="true"/>, then every log is encriched with the logging <see cref="Object"/> instance's hierarchy name.
    /// </summary>
    /// <remarks>
    /// For <see cref="GameObject"/> and <see cref="Component"/>-derived instances, the hierarchy name is
    /// the name of the object's transform and all parent transforms, separated by <see cref="ParentNameSeparator"/>.
    /// For all other <see cref="Object"/> instances, the hierarchy name is simply the object's <see cref="Object.name"/>.
    /// Note that computing this name requires walking the logging <see cref="Object"/>'s transform hierarchy on every log event,
    /// which could get expensive for deeply childed objects.
    /// If the object's transform hierarchy does not change over the course of its lifetime then
    /// set <see cref="HasStaticHierarchy"/> to <see langword="true"/> so that the hierarchy name can be cached.
    /// </remarks>
    public bool EnrichWithHierarchyName { get; set; } = false;

    /// <summary>
    /// Name of the log property that holds the logging <see cref="Object"/> instance's hierarchy name.
    /// </summary>
    /// <remarks><inheritdoc cref="EnrichWithHierarchyName" path="/remarks"/></remarks>
    public string HierarchyNameLogProperty { get; set; } = "UnityHierarchyName";

    /// <summary>
    /// Whether the logging <see cref="Object"/> instance's hierarchy is static.
    /// I.e., for <see cref="GameObject"/> and <see cref="Component"/>-derived instances, whether its parent transforms ever change.
    /// If an object's hierarchy is static, then its hierarchy name is cached for better logging performance.
    /// </summary>
    public bool HasStaticHierarchy { get; set; } = true;

    /// <summary>
    /// For logging <see cref="Object"/>s that are <see cref="GameObject"/>s or derived from <see cref="Component"/>,
    /// the object's transform and all of its parent transforms will be separated by this string.
    /// Prefer single-character separators to keep rendered log messages small.
    /// </summary>
    public string ParentNameSeparator { get; set; } = ">";  // Better default than '/', so the hierarchy name isn't mistaken for a file path
}
