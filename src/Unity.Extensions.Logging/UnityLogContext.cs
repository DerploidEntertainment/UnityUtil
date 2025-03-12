using UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// Encapsulates a <see cref="Object"/> instance to be used as the <c>context</c> parameter of a Unity log message.
/// Allows custom Serilog destructuring policies to reference this type rather than, e.g., instances of all <see cref="Object"/>-derived types.
/// See Unity's <a href="https://docs.unity3d.com/ScriptReference/Debug.Log.html"><c>Debug.Log</c></a> docs for a description of the <c>context</c> parameter.
/// </summary>
/// <param name="context"><inheritdoc cref="Context" path="/summary"/></param>
internal struct UnityLogContext(Object context)
{
    /// <summary>
    /// The encapsulated <see cref="Object"/> instance used for the Unity log context.
    /// </summary>
    public readonly Object Context => context;
}
