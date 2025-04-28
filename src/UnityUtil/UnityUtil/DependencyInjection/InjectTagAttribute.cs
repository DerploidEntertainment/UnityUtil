using System;
using UnityEngine;

namespace UnityUtil.DependencyInjection;

/// <summary>
/// Inject the service configured with this field's <see cref="Type"/> and an optional Inspector tag.
/// </summary>
/// <remarks></remarks>
/// <param name="tag">The service <see cref="UnityEngine.Object"/> with this tag (set in the Inspector) will be injected.  Use when registering multiple services with the same Type.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class InjectTagAttribute(string tag) : PropertyAttribute
{
    /// <summary>
    /// The service <see cref="UnityEngine.Object"/> with this tag (set in the Inspector) will be injected.  Use when registering multiple services with the same Type.
    /// </summary>
    public string Tag { get; } = tag;
}
