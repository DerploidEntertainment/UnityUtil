using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using System;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="MEL.ILogger"/> to wrap Unity's own <see cref="Debug"/> <c>Log*</c> methods.
/// Mainly intended for Unity code that can't set up a proper logging framework via dependency injection (e.g., in Editor scripts).
/// Most runtime code should still prefer dependency injection and one of the custom <see cref="UnityUtil"/> loggers.
/// </summary>
public class UnityDebugLogger : MEL.ILogger
{
    public UnityDebugLogger() { }

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        string msg = $"{eventId} {formatter(state, exception)}";

        switch (logLevel) {
            case LogLevel.None:
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Information:
                Debug.Log(msg);
                break;

            case LogLevel.Warning:
                Debug.LogWarning(msg);
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                Debug.LogError(msg);
                break;

            default:
                throw UnityObjectExtensions.SwitchDefaultException(logLevel);
        }
    }
}

