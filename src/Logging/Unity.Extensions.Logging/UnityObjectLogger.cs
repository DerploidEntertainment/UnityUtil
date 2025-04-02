using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;
using UE = UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// An <see cref="MEL.ILogger"/> that adds a <see cref="UE.Object"/>-derived instance as a scope property
/// to all of its log messages for use by Unity-specific logging providers.
/// </summary>
/// <typeparam name="T">A logging <see cref="UE.Object"/>-derived type</typeparam>
public class UnityObjectLogger<T> : ILogger<T> where T : UE.Object
{
    private readonly ILogger<T> _logger;
    private readonly UnityObjectLoggerSettings _unityObjectLoggerSettings;

    private Dictionary<string, object>? _scopeProps;
    private readonly string? _contextName;
    private List<string>? _hierarchyNameParts;
    private readonly Transform? _transform = null;

    /// <summary>
    /// Creates a new instance of <see cref="UnityObjectLogger{T}"/>.
    /// </summary>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory" path="/summary"/></param>
    /// <param name="context">The <typeparamref name="T"/> instance to add as a scope property to all log messages for use by Unity-specific logging providers.</param>
    /// <param name="unityObjectLoggerSettings"><inheritdoc cref="UnityObjectLoggerSettings" path="/summary"/></param>
    public UnityObjectLogger(
        ILoggerFactory loggerFactory,
        T context,
        UnityObjectLoggerSettings? unityObjectLoggerSettings = null
    )
    {
        _logger = loggerFactory.CreateLogger<T>();
        _unityObjectLoggerSettings = unityObjectLoggerSettings ?? new UnityObjectLoggerSettings();

        _contextName = context.name;

        if (_unityObjectLoggerSettings.AddUnityContext) {
            _scopeProps ??= [];
            _scopeProps.Add($"@{_unityObjectLoggerSettings.UnityContextLogProperty}", ValueTuple.Create(context));
        }

        if (_unityObjectLoggerSettings.AddHierarchyName) {
            _transform =
                context is GameObject gameObject ? gameObject.transform
                : context is Component component ? component.transform
                : null;
            if (_transform != null && _unityObjectLoggerSettings.HasStaticHierarchy) {
                _scopeProps ??= [];
                _scopeProps.Add(_unityObjectLoggerSettings.HierarchyNameLogProperty, GetHierarchyName());

                // Null out fields that won't be needed again with static hierarchies
                _hierarchyNameParts = null;
                _contextName = null;
            }
        }
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_unityObjectLoggerSettings.AddHierarchyName && !_unityObjectLoggerSettings.HasStaticHierarchy) {
            _scopeProps ??= [];
            _scopeProps[_unityObjectLoggerSettings.HierarchyNameLogProperty] = GetHierarchyName();
        }

        if (_scopeProps is null)
            _logger.Log(logLevel, eventId, state, exception, formatter);
        else {
            using (_logger.BeginScope(_scopeProps))
                _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    /// <summary>
    /// Gets the hierarchy name of the logging <see cref="UE.Object"/> instance.
    /// </summary>
    /// <remarks><inheritdoc cref="UnityObjectLoggerSettings.AddHierarchyName" path="/remarks"/></remarks>
    /// <returns>The hierarchy name of the logging <see cref="UE.Object"/> instance.</returns>
    public string GetHierarchyName()
    {
        if (_transform is null)
            return _contextName!;   // Not set to null in ctor if hierarchy is dynamic

        if (_hierarchyNameParts is null)
            _hierarchyNameParts = [];
        else
            _hierarchyNameParts.Clear(); // Should be faster and generate less garbage than newing up a whole new list, at least for shallow-ish hierarchies

        Transform trans = _transform;
        do {
            _hierarchyNameParts.Add(trans.name);
            trans = trans.parent;
        } while (trans != null);
        _hierarchyNameParts.Reverse();

        return string.Join(_unityObjectLoggerSettings.ParentNameSeparator, _hierarchyNameParts);
    }
}
