using System;
using Microsoft.Extensions.Logging;

namespace UnityUtil.Logging;

/// <summary>
/// "Default" implementation of <see cref="ILoggerFactory"/> that creates instances of <see cref="UnityDebugLogger"/>.
/// </summary>
public class UnityDebugLoggerFactory : ILoggerFactory
{
    public UnityDebugLoggerFactory() { }

    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new UnityDebugLogger();
    public void Dispose() => GC.SuppressFinalize(this);
}

