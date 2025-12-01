using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtil;

public static class UnityObjectExtensions
{
    private const int DEFAULT_PARENT_COUNT = 1;
    private const string DEFAULT_SEPARATOR = "/";

    /// <summary><inheritdoc cref="GetHierarchyName(Component, int, string)"/></summary>
    /// <remarks><inheritdoc cref="GetHierarchyName(Component, int, string)"/></remarks>
    /// <param name="obj"><inheritdoc cref="GetHierarchyName(Component, int, string)" path="/param[@name='obj']"/></param>
    /// <param name="parentCount"><inheritdoc cref="GetHierarchyName(Component, int, string)" path="/param[@name='parentCount']"/></param>
    /// <param name="separator"><inheritdoc cref="GetHierarchyName(Component, int, string)" path="/param[@name='separator']"/></param>
    /// <returns><inheritdoc cref="GetHierarchyName(Component, int, string)"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"><inheritdoc cref="GetHierarchyName(Component, int, string)"/></exception>
    public static string GetHierarchyName(
        this GameObject obj,
        int parentCount = DEFAULT_PARENT_COUNT,
        string separator = DEFAULT_SEPARATOR
    ) => GetHierarchyName(obj.transform, parentCount, separator);

    /// <summary>
    /// Gets <paramref name="obj"/>'s "hierarchy name".
    /// That is, its <see cref="UnityEngine.Object.name"/> concatenated with the names of up to <paramref name="parentCount"/> parents above it,
    /// separated by <paramref name="separator"/>.
    /// For example, <c>parent/object</c> or <c>root > group > object</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <paramref name="parentCount"/> is 0, then only <paramref name="obj"/>'s name is returned.
    /// If <paramref name="obj"/> has fewer parents than <paramref name="parentCount"/>,
    /// then the hierarchy name will only contain the names of as many parents as <paramref name="obj"/> actually has.
    /// To include all parent names, set <paramref name="parentCount"/> to something high, e.g., <see cref="int.MaxValue"/>.
    /// </para>
    /// <para>
    /// <strong>Warning</strong>: This method walks the Transform hierarchy to find <paramref name="obj"/>'s parents.
    /// For deeply nested GameObjects, you may want to use a smaller <paramref name="parentCount"/> inside performance-critical code.
    /// You could also cache the hierarchy name, but the name may become stale if <paramref name="obj"/>'s hierarchy changes.
    /// </para>
    /// </remarks>
    /// <param name="obj"></param>
    /// <param name="parentCount"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="separator"/> is negative.</exception>
    public static string GetHierarchyName(
        this Component obj,
        int parentCount = DEFAULT_PARENT_COUNT,
        string separator = DEFAULT_SEPARATOR
    )
    {
        if (parentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(parentCount), "Must be greater than or equal to zero");

        List<string> transformNames = [obj.name];

        Transform? parent = obj.transform.parent;
        for (int p = 0; p < parentCount; ++p) {
            if (parent == null)
                break;
            transformNames.Add(parent.name);
            parent = parent.parent;
        }

        transformNames.Reverse();
        return string.Join(separator, transformNames);
    }

    /// <summary>
    /// Assert that this component is both active and enabled.
    /// </summary>
    /// <param name="behaviour">The <see cref="Behaviour"/> to assert as active and enabled</param>
    /// <param name="verbMessage">
    /// If this component is either inactive or disabled, then this verb will be used in the logged error message.
    /// Should be present-tense phrase, like "stop", or "perform that action". Padding spaces are not required.
    /// </param>
    public static void ThrowIfInactiveOrDisabled(this Behaviour behaviour, string verbMessage = "use")
    {
        string hierarchyNameWithType = $"{behaviour.GetType().Name} '{behaviour.GetHierarchyName()}'";
        if (!behaviour.gameObject.activeInHierarchy)
            throw new InvalidOperationException($"Cannot {verbMessage} {hierarchyNameWithType} because its GameObject is inactive");
        if (!behaviour.enabled)
            throw new InvalidOperationException($"Cannot {verbMessage} {hierarchyNameWithType} because it is disabled");
    }

    public static InvalidOperationException SwitchDefaultException<T>(T value) where T : Enum =>
        new($"Gah! We haven't accounted for {typeof(T).Name} {value} yet!");
}
