using System;
using Serilog;
using Serilog.Configuration;

namespace Unity.Extensions.Serilog;

/// <summary>
/// Extends <see cref="LoggerDestructuringConfiguration"/> for logging in the Unity engine.
/// </summary>
public static class LoggerDestructuringConfigurationExtensions
{
    /// <summary>
    /// Treat <see cref="UnityEngine.Object"/> instances wrapped in <see cref="ValueTuple{T}"/> as scalar values,
    /// i.e., don't break them down into properties event when destructuring complex types.
    /// This allows them to be used as the <c>context</c> for logs in the Unity engine.
    /// </summary>
    /// <param name="loggerDestructuringConfiguration">Logger destructuring configuration.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration UnityObjectContext(this LoggerDestructuringConfiguration loggerDestructuringConfiguration) =>
        loggerDestructuringConfiguration is null
        ? throw new ArgumentNullException(nameof(loggerDestructuringConfiguration))
        : loggerDestructuringConfiguration.With(new UnityLogContextDestructuringPolicy());
}
