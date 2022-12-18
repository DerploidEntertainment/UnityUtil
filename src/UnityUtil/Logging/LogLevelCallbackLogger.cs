using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using System;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="MEL.ILogger"/> that calls provided callbacks for log messages based on their <see cref="LogLevel"/>.
/// Mainly intended for testing code that needs to verify that loggers were invoked in specific ways.
/// Most runtime code should still prefer dependency injection and one of the custom <see cref="UnityUtil"/> loggers.
/// </summary>
public class LogLevelCallbackLogger : ILogger
{
    private readonly LogLevel _level;
    private readonly Action<LogLevel, EventId, Exception, string> _levelCallback;
    private readonly Action<LogLevel, EventId, Exception, string>? _alwaysCallback;

    public LogLevelCallbackLogger(
        LogLevel level,
        Action<LogLevel, EventId, Exception, string> levelCallback,
        Action<LogLevel, EventId, Exception, string>? alwaysCallback = null
    )
    {
        _level = level;
        _levelCallback = levelCallback;
        _alwaysCallback = alwaysCallback;
    }

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        string msg = formatter(state, exception);
        if (logLevel == _level)
            _levelCallback(logLevel, eventId, exception, msg);
        _alwaysCallback?.Invoke(logLevel, eventId, exception, msg);
    }
}
