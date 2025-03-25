using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;
using UE = UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// An <see cref="MEL.ILogger"/> that adds a <see cref="UE.Object"/> instance as a scope property
/// to all of its log messages for use by Unity-specific logging providers.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnityObjectLogger<T> : ILogger<T> where T : UE.Object
{
    private readonly ILogger<T> _logger;
    private readonly UnityObjectLoggerSettings _unityObjectLoggerSettings;

    private Dictionary<string, object>? _scopeProps;
    private readonly string? _contextName;
    private List<string>? _hierarchicalNameParts;
    private readonly Transform? _transform = null;

    /// <summary>
    /// Creates a new instance of <see cref="UnityObjectLogger{T}"/>.
    /// </summary>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory" path="/summary"/></param>
    /// <param name="context">The <see cref="UE.Object"/> instance to add as a scope property to all log messages for use by Unity-specific logging providers.</param>
    /// <param name="unityObjectLoggerSettings"><inheritdoc cref="UnityObjectLoggerSettings" path="/summary"/></param>
    public UnityObjectLogger(
        ILoggerFactory loggerFactory,
        UE.Object context,
        UnityObjectLoggerSettings? unityObjectLoggerSettings = null
    )
    {
        _logger = loggerFactory.CreateLogger<T>();
        _unityObjectLoggerSettings = unityObjectLoggerSettings ?? new UnityObjectLoggerSettings();

        _contextName = context.name;

        if (_unityObjectLoggerSettings.UnityContextLogProperty is not null) {
            _scopeProps ??= [];
            _scopeProps.Add($"@{_unityObjectLoggerSettings.UnityContextLogProperty}", new UnityLogContext(context));
        }

        if (_unityObjectLoggerSettings.HierarchicalNameLogProperty is not null && _unityObjectLoggerSettings.HasStaticHierarchy) {
            _transform =
                context is GameObject gameObject ? gameObject.transform
                : context is Component component ? component.transform
                : null;
            _scopeProps ??= [];
            _scopeProps.Add(_unityObjectLoggerSettings.HierarchicalNameLogProperty, getHierarchicalName());

            // Null out fields that won't be needed again with static hierarchies
            _hierarchicalNameParts = null;
            _contextName = null;
        }
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_unityObjectLoggerSettings.HierarchicalNameLogProperty is not null && !_unityObjectLoggerSettings.HasStaticHierarchy) {
            _scopeProps ??= [];
            _scopeProps[_unityObjectLoggerSettings.HierarchicalNameLogProperty] = getHierarchicalName();
        }

        if (_scopeProps is null)
            _logger.Log(logLevel, eventId, state, exception, formatter);
        else {
            using (_logger.BeginScope(_scopeProps))
                _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private string getHierarchicalName()
    {
        if (_transform is null)
            return _contextName!;   // Not set to null in ctor if hierarchy is dynamic

        if (_hierarchicalNameParts is null)
            _hierarchicalNameParts = [];
        else
            _hierarchicalNameParts.Clear(); // Should be faster and generate less garbage than newing up a whole new list, at least for shallow-ish hierarchies

        Transform trans = _transform;
        do {
            _hierarchicalNameParts.Add(trans.name + "");
            trans = trans.parent;
        } while (trans != null);
        _hierarchicalNameParts.Reverse();

        return string.Join(_unityObjectLoggerSettings.ParentNameSeparator, _hierarchicalNameParts);
    }
}
