using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Unity.Extensions.Logging;

public static class LoggerFactoryExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory" path="/summary"/></param>
    /// <param name="context"><inheritdoc cref="UnityObjectLogger{T}.UnityObjectLogger(ILoggerFactory, Object, UnityObjectLoggerSettings?)" path="/param[@name='context']"/></param>
    /// <param name="unityObjectLoggerSettings"><inheritdoc cref="UnityObjectLogger{T}.UnityObjectLogger(ILoggerFactory, Object, UnityObjectLoggerSettings?)" path="/param[@name='unityObjectLoggerSettings']"/></param>
    /// <returns>
    /// A <see cref="UnityObjectLogger{T}"/> that adds <paramref name="context"/> as a scope property
    /// to all of its log messages for use by Unity-specific logging providers.
    /// </returns>
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory, T context, UnityObjectLoggerSettings? unityObjectLoggerSettings = null) where T : Object =>
        new UnityObjectLogger<T>(loggerFactory, context, unityObjectLoggerSettings);
}
