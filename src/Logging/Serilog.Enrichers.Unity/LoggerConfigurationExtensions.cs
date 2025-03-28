using System;
using Serilog.Configuration;

namespace Serilog.Enrichers.Unity;

public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Enriches log events with context from the Unity engine, such as frame counts.
    /// </summary>
    /// <param name="enrichmentConfiguration">Logger sink configuration.</param>
    /// <param name="unityLogEnricherSettings"><inheritdoc cref="UnityLogEnricher(UnityLogEnricherSettings)" path="/param[@name='unityLogEnricherSettings']"/></param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithUnityData(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        UnityLogEnricherSettings unityLogEnricherSettings
    ) =>
        enrichmentConfiguration is null ? throw new ArgumentNullException(nameof(enrichmentConfiguration))
        : unityLogEnricherSettings is null ? throw new ArgumentNullException(nameof(unityLogEnricherSettings))
        : enrichmentConfiguration.With(new UnityLogEnricher(unityLogEnricherSettings));
}
