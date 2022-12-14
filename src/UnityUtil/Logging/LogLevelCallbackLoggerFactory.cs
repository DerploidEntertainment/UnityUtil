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
    private readonly Action _callback;

    public LogLevelCallbackLoggerFactory(LogLevel level, Action callback)
    {
        _level = level;
        _callback = callback;
    }

    public void AddProvider(MEL.ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new LogLevelCallbackLogger(_level, _callback);
    public void Dispose() => GC.SuppressFinalize(this);
}
