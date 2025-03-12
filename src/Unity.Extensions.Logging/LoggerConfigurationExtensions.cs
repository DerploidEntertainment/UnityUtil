using Serilog;
using Serilog.Debugging;
using Serilog.Enrichers.Unity;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Unity;
using UnityEngine;

namespace Unity.Extensions.Logging;

/// <summary>
/// Extends <see cref="LoggerConfiguration"/> with methods to add Unity enricher, sink, and other configuration.
/// </summary>
public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Add the Unity Console to the Serilog pipeline.
    /// </summary>
    /// <param name="loggerConfiguration">Logger configuration.</param>
    /// <param name="textFormatter"><inheritdoc cref="UnitySink(ITextFormatter, UnitySinkSettings)" path="/param[@name='textFormatter']"/></param>
    /// <param name="selfLogIsUnityLogWarning">
    /// Whether Serilog's <see cref="SelfLog"/> is set to Unity's <see cref="Debug.LogWarning(object)"/>.
    /// Useful during development, but may generate noisy warnings if trying to destructure <see cref="Object"/>s in <see cref="LogEvent"/>s.
    /// </param>
    /// <param name="setMinimumLevelFromUnityFilterLogType">
    /// Whether to set <see cref="LoggerConfiguration.MinimumLevel"/> based on Unity's <see cref="UnityEngine.ILogger.filterLogType"/>.
    /// </param>
    /// <param name="unityLogEnricherSettings"><inheritdoc cref="UnityLogEnricher(UnityLogEnricherSettings)" path="/param[@name='unityLogEnricherSettings']"/></param>
    /// <param name="unitySinkSettings">
    /// <inheritdoc cref="UnitySink(ITextFormatter, UnitySinkSettings)" path="/param[@name='unitySinkSettings']"/>
    /// By default, <see cref="UnitySinkSettings.UnityTagLogProperty"/> is set to <c>SourceContext</c> to reuse the type name set by <c>Microsoft.Extensions.Logging</c>
    /// as the Unity log <c>tag</c>.
    /// </param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration AddUnity(
        this LoggerConfiguration loggerConfiguration,
        ITextFormatter textFormatter,
        bool selfLogIsUnityLogWarning = true,
        bool setMinimumLevelFromUnityFilterLogType = true,
        UnityLogEnricherSettings? unityLogEnricherSettings = null,
        UnitySinkSettings? unitySinkSettings = null
    )
    {
        unitySinkSettings ??= new UnitySinkSettings {
            UnityTagLogProperty = "SourceContext",  // This log property is added by
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

        if (unitySinkSettings.UnityContextLogProperty is not null)
            loggerConfiguration = loggerConfiguration.Destructure.With(new UnityLogContextDestructuringPolicy());

        return loggerConfiguration
            .Enrich.WithUnityData(unityLogEnricherSettings ?? new UnityLogEnricherSettings())
            .WriteTo.Unity(textFormatter, unitySinkSettings);
    }
}
