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
    /// <param name="context"><inheritdoc cref="UnityContextLogger{T}.UnityContextLogger(ILoggerFactory, Object, string?)" path="/param[@name='context']"/></param>
    /// <returns>
    /// A <see cref="UnityContextLogger{T}"/> that adds <paramref name="context"/> as a scope property
    /// to all of its log messages for use by Unity-specific logging providers.
    /// </returns>
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory, T context) where T : Object =>
        new UnityContextLogger<T>(loggerFactory, context);
}
