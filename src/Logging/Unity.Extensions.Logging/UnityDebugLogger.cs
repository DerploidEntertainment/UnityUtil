using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging;

/// <summary>
/// "Default" implementation of <see cref="MEL.ILogger"/> to wrap Unity's own <see cref="Debug"/> <c>Log*</c> methods.
/// Mainly intended for Unity code that can't set up a proper logging framework via dependency injection (e.g., in Editor scripts).
/// Most runtime code should still prefer injecting an <see cref="ILoggerFactory"/> and creating an <see cref="ILogger{TCategoryName}"/>.
/// </summary>
public class UnityDebugLogger : MEL.ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string msg = $"{eventId} {formatter(state, exception)}";

        LogType logType = logLevel switch {
            LogLevel.None or LogLevel.Trace or LogLevel.Debug or LogLevel.Information => LogType.Log,
            LogLevel.Warning => LogType.Warning,
            LogLevel.Error or LogLevel.Critical => LogType.Error,
            _ => throw new NotImplementedException($"Unknown {nameof(LogLevel)}: {logLevel}")
        };

        Debug.unityLogger.Log(logType, msg);
    }
}

