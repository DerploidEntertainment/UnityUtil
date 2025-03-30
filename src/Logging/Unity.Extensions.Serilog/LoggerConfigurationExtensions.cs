using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Enrichers.Unity;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Unity;
using UnityEngine;
using UE = UnityEngine;

namespace Unity.Extensions.Serilog;

/// <summary>
/// Extends <see cref="LoggerConfiguration"/> with methods to add Unity enricher, sink, and other configuration.
/// </summary>
public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Add the Unity Console to the Serilog pipeline.
    /// </summary>
    /// <param name="loggerConfiguration">Logger configuration.</param>
    /// <param name="textFormatter"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='textFormatter']"/></param>
    /// <param name="logger"><inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='logger']"/></param>
    /// <param name="selfLogIsUnityLogWarning">
    /// Whether Serilog's <see cref="SelfLog"/> is set to Unity's <see cref="Debug.LogWarning(object)"/>.
    /// Useful during development, but may generate noisy warnings if trying to destructure <see cref="Object"/>s in <see cref="LogEvent"/>s.
    /// </param>
    /// <param name="setMinimumLevelFromUnityFilterLogType">
    /// Whether to set <see cref="LoggerConfiguration.MinimumLevel"/> based on Unity's <see cref="UE.ILogger.filterLogType"/>.
    /// </param>
    /// <param name="useUnityEnricher">Whether to use the <see cref="UnityLogEnricher"/> to enrich <see cref="LogEvent"/>s with Unity-specific properties.</param>
    /// <param name="useUnitySink">Whether to use the <see cref="UnitySink"/> to write <see cref="LogEvent"/>s out to the Unity Debug Console (and associated log files).</param>
    /// <param name="unityLogEnricherSettings"><inheritdoc cref="UnityLogEnricher(UnityLogEnricherSettings)" path="/param[@name='unityLogEnricherSettings']"/></param>
    /// <param name="unitySinkSettings">
    /// <inheritdoc cref="UnitySink(ITextFormatter, UE.ILogger, UnitySinkSettings)" path="/param[@name='unitySinkSettings']"/>
    /// By default, <see cref="UnitySinkSettings.UnityTagLogProperty"/> is set to <see cref="Constants.SourceContextPropertyName"/> to reuse the type name set by <c>Serilog</c>
    /// as the Unity log <c>tag</c>.
    /// </param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration AddUnity(
        this LoggerConfiguration loggerConfiguration,
        ITextFormatter textFormatter,
        UE.ILogger? logger = null,
        bool selfLogIsUnityLogWarning = true,
        bool setMinimumLevelFromUnityFilterLogType = true,
        bool useUnityEnricher = true,
        bool useUnitySink = true,
        UnityLogEnricherSettings? unityLogEnricherSettings = null,
        UnitySinkSettings? unitySinkSettings = null
    )
    {
        unitySinkSettings ??= new UnitySinkSettings {
            UnityTagLogProperty = Constants.SourceContextPropertyName,
        };

        if (selfLogIsUnityLogWarning)
            SelfLog.Enable(Debug.LogWarning);

        if (setMinimumLevelFromUnityFilterLogType) {
            loggerConfiguration = loggerConfiguration.MinimumLevel.Is(
                Debug.unityLogger.filterLogType switch {
                    LogType.Log => LogEventLevel.Information,
                    LogType.Warning => LogEventLevel.Warning,
                    LogType.Assert or LogType.Error or LogType.Exception => LogEventLevel.Error,
                    _ => LogEventLevel.Information
                }
            );
        }

        if (useUnitySink && unitySinkSettings.UnityContextLogProperty is not null)
            loggerConfiguration = loggerConfiguration.Destructure.With(new UnityLogContextDestructuringPolicy());

        if (useUnityEnricher)
            loggerConfiguration = loggerConfiguration.Enrich.WithUnityData(unityLogEnricherSettings ?? new UnityLogEnricherSettings());

        if (useUnitySink)
            loggerConfiguration = loggerConfiguration.WriteTo.Unity(textFormatter, logger, unitySinkSettings);

        return loggerConfiguration;
    }
}
