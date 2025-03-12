using System;
using Microsoft.Extensions.Logging;

namespace UnityUtil.Tests.Util;

/// <summary>
/// "Default" implementation of <see cref="ILoggerFactory"/> that creates instances of <see cref="LogLevelCallbackLogger"/>.
/// </summary>
public class LogLevelCallbackLoggerFactory(
    LogLevel level,
    Action<LogLevel, EventId, Exception?, string> levelCallback,
    Action<LogLevel, EventId, Exception?, string>? alwaysCallback = null
) : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new LogLevelCallbackLogger(level, levelCallback, alwaysCallback);
    public void Dispose() => GC.SuppressFinalize(this);
}
