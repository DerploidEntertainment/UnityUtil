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
    private bool _disposed;

    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new LogLevelCallbackLogger(level, levelCallback, alwaysCallback);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
