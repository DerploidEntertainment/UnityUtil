using System;
using Microsoft.Extensions.Logging;

namespace UnityUtil.Tests.Util;

/// <summary>
/// "Default" implementation of <see cref="ILogger"/> that calls provided callbacks for log messages based on their <see cref="LogLevel"/>.
/// Mainly intended for testing code that needs to verify that loggers were invoked in specific ways.
/// Most runtime code should still prefer dependency injection and one of the custom <see cref="UnityUtil"/> loggers.
/// </summary>
public class LogLevelCallbackLogger(
    LogLevel level,
    Action<LogLevel, EventId, Exception?, string> levelCallback,
    Action<LogLevel, EventId, Exception?, string>? alwaysCallback = null
) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string msg = formatter(state, exception);
        if (logLevel == level)
            levelCallback(logLevel, eventId, exception, msg);
        alwaysCallback?.Invoke(logLevel, eventId, exception, msg);
    }
}
