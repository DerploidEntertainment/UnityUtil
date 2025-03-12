using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using UE = UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// An <see cref="ILogger"/> that adds a <see cref="UE.Object"/> instance as a scope property
/// to all of its log messages for use by Unity-specific logging providers.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnityContextLogger<T> : ILogger<T> where T : UE.Object
{
    private readonly ILogger<T> _logger;
    private readonly string? _unityContextLogProperty;

    /// <summary>
    /// Re-using this dictionary saves us an allocation per <see cref="LogEvent"/>.
    /// </summary>
    private readonly Dictionary<string, object>? _scopeProps;

    /// <summary>
    /// Creates a new instance of <see cref="UnityContextLogger{T}"/>.
    /// </summary>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory" path="/summary"/></param>
    /// <param name="context">
    /// The <see cref="UE.Object"/> instance to add as a scope property to all log messages for use by Unity-specific logging providers.
    /// </param>
    /// <param name="unityContextLogProperty">
    /// The name of the log property that will hold <paramref name="context"/> on all log messages for use by Unity-specific logging providers.
    /// </param>
    public UnityContextLogger(
        ILoggerFactory loggerFactory,
        UE.Object context,
        string? unityContextLogProperty = "UnityLogContext"
    )
    {
        _logger = loggerFactory.CreateLogger<T>();
        _unityContextLogProperty = unityContextLogProperty;

        if (_unityContextLogProperty is not null) {
            _scopeProps = new() {
                { $"@{_unityContextLogProperty}", new UnityLogContext(context) }
            };
        }
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_scopeProps is null)
            _logger.Log(logLevel, eventId, state, exception, formatter);
        else {
            using (_logger.BeginScope(_scopeProps))
                _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
