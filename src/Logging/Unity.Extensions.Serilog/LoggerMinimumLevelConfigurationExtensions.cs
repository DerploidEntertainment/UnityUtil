using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using UnityEngine;
using UE = UnityEngine;

namespace Unity.Extensions.Serilog;

/// <summary>
/// Extends <see cref="LoggerMinimumLevelConfigurationExtensions"/> for logging in the Unity engine.
/// </summary>
public static class LoggerMinimumLevelConfigurationExtensions
{
    /// <summary>
    /// Add the Unity Console to the Serilog pipeline.
    /// </summary>
    /// <param name="loggerMinimumLevelConfiguration">Logger minimum level configuration.</param>
    /// <param name="logger">
    /// The <see cref="UE.ILogger"/> from which to set the minimum <see cref="LogEventLevel"/>,
    /// based on its <see cref="UE.ILogger.filterLogType"/>.
    /// If <see langword="null"/> then <see cref="Debug.unityLogger"/> is used.
    /// </param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration IsUnityFilterLogType(this LoggerMinimumLevelConfiguration loggerMinimumLevelConfiguration, UE.ILogger? logger = null) =>
        loggerMinimumLevelConfiguration is null
        ? throw new ArgumentNullException(nameof(loggerMinimumLevelConfiguration))
        : loggerMinimumLevelConfiguration.Is(
            (logger ?? Debug.unityLogger).filterLogType switch {
                LogType.Log => LogEventLevel.Information,
                LogType.Warning => LogEventLevel.Warning,
                LogType.Assert or LogType.Error or LogType.Exception => LogEventLevel.Error,
                _ => LogEventLevel.Information
            }
        );
}
