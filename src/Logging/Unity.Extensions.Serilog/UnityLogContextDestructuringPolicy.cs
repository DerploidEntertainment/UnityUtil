using System;
using System.Diagnostics.CodeAnalysis;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;
using UE = UnityEngine;

namespace Unity.Extensions.Serilog;

/// <summary>
/// Policy for destructuring <see cref="ValueTuple{T}"/> instances with a single <see cref="UE.Object"/> item, representing the <c>context</c> for a Unity log.
/// <para>
/// Provides a mechanism for preserving <see cref="UE.Object"/> instances added to a <see cref="LogEvent"/>'s properties (e.g., via <see cref="LogContext"/>)
/// so that the instance may be used as the <c>context</c> of a Unity <see cref="Debug.LogWarning(object, UE.Object)"/> message
/// (without affecting the destructuring of other <see cref="UE.Object"/> log property values set by API consumers).
/// </para>
/// </summary>
public sealed class UnityLogContextDestructuringPolicy : IDestructuringPolicy
{
    /// <inheritdoc />
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value is ValueTuple<UE.Object> context) {
            result = new ScalarValue(context.Item1);
            return true;
        }

        result = null;
        return false;
    }
}
