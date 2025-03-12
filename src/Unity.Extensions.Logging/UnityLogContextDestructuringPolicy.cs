using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging;

/// <summary>
/// Policy for destructuring <see cref="UnityLogContext"/> instances.
/// <para>
/// Provides a mechanism for preserving <see cref="Object"/> instances added to a <see cref="LogEvent"/>'s
/// properties via <c>Microsoft.Extensions.Logging</c>'s <see cref="MEL.LoggerExtensions.BeginScope(MEL.ILogger, string, object?[])"/> method,
/// so that the instance may be used as the <c>context</c> of a Unity <see cref="Debug.LogWarning(object, Object)"/> message
/// (without affecting the destructuring of other <see cref="Object"/> log property values set by API consumers).
/// </para>
/// </summary>
internal class UnityLogContextDestructuringPolicy : IDestructuringPolicy
{
    /// <inheritdoc />
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value is UnityLogContext context) {
            result = new ScalarValue(context.Context);
            return true;
        }

        result = null;
        return false;
    }
}
