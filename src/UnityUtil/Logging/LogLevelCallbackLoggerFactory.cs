using Microsoft.Extensions.Logging;
using System;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="ILoggerFactory"/> that creates instances of <see cref="LogLevelCallbackLogger"/>.
/// </summary>
public class LogLevelCallbackLoggerFactory(
    LogLevel level,
    Action<LogLevel, EventId, Exception?, string> levelCallback,
    Action<LogLevel, EventId, Exception?, string>? alwaysCallback = null
) : ILoggerFactory
{
    public void AddProvider(MEL.ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new LogLevelCallbackLogger(level, levelCallback, alwaysCallback);
    public void Dispose() => GC.SuppressFinalize(this);
}
