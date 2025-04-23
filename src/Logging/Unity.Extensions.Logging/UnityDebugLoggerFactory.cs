using System;
using Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging;

/// <summary>
/// "Default" implementation of <see cref="ILoggerFactory"/> that creates instances of <see cref="UnityDebugLogger"/>.
/// </summary>
public class UnityDebugLoggerFactory : ILoggerFactory
{
    private bool _disposed;

    /// <inheritdoc/>
    public void AddProvider(ILoggerProvider provider) { }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new UnityDebugLogger();

    /// <summary>
    /// Implementation of .NET Dispose pattern
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> if called from <see cref="Dispose()"/>; otherwise, <see langword="false"/>.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

