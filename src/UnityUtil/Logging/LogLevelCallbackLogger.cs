using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using System;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="MEL.ILogger"/> that calls a callback for every log message with a specified <see cref="LogLevel"/>.
/// Mainly intended for testing code that needs to verify that loggers were invoked in specific ways.
/// Most runtime code should still prefer dependency injection and one of the custom <see cref="UnityUtil"/> loggers.
/// </summary>
public class LogLevelCallbackLogger : ILogger
{
    private readonly LogLevel _level;
    private readonly Action _callback;

    public LogLevelCallbackLogger(LogLevel level, Action callback)
    {
        _level = level;
        _callback = callback;
    }

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (logLevel == _level)
            _callback();
    }
}
