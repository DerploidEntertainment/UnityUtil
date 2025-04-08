using System;
using Microsoft.Extensions.Logging;

namespace Unity.Extensions.Logging;

/// <summary>
/// Extends <see cref="ILoggerFactory"/> with methods for creating Unity-specific loggers.
/// </summary>
public static class LoggerFactoryExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory" path="/summary"/></param>
    /// <param name="context"><inheritdoc cref="UnityObjectLogger{T}.UnityObjectLogger(ILoggerFactory, T, UnityObjectLoggerSettings?)" path="/param[@name='context']"/></param>
    /// <param name="unityObjectLoggerSettings"><inheritdoc cref="UnityObjectLogger{T}.UnityObjectLogger(ILoggerFactory, T, UnityObjectLoggerSettings?)" path="/param[@name='unityObjectLoggerSettings']"/></param>
    /// <returns>
    /// A <see cref="UnityObjectLogger{T}"/> that adds <paramref name="context"/> as a scope property
    /// to all of its log messages for use by Unity-specific logging providers.
    /// </returns>
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory, T context, UnityObjectLoggerSettings? unityObjectLoggerSettings = null) where T : UnityEngine.Object =>
        loggerFactory is null
        ? throw new ArgumentNullException(nameof(loggerFactory))
        : new UnityObjectLogger<T>(loggerFactory, context, unityObjectLoggerSettings);
}
