using Microsoft.Extensions.Logging;
using System;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="ILoggerFactory"/> that creates instances of <see cref="LogLevelCallbackLogger"/>.
/// </summary>
public class LogLevelCallbackLoggerFactory : ILoggerFactory
{
    private readonly LogLevel _level;
    private readonly Action<LogLevel, EventId, Exception, string> _levelCallback;
    private readonly Action<LogLevel, EventId, Exception, string>? _alwaysCallback;

    public LogLevelCallbackLoggerFactory(
        LogLevel level,
        Action<LogLevel, EventId, Exception, string> levelCallback,
        Action<LogLevel, EventId, Exception, string>? alwaysCallback = null
    )
    {
        _level = level;
        _levelCallback = levelCallback;
        _alwaysCallback = alwaysCallback;
    }

    public void AddProvider(MEL.ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new LogLevelCallbackLogger(_level, _levelCallback, _alwaysCallback);
    public void Dispose() => GC.SuppressFinalize(this);
}
