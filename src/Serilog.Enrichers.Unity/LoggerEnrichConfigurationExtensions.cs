using System;

namespace Serilog.Enrichers.Unity;

public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enriches log events with context from the Unity engine such as frame counts, GameObject hierarchies, etc.
    /// For assets (e.g., <see cref="UnityEngine.ScriptableObject"/>s), this will simply be the <see cref="UnityEngine.Object.name"/>.
    /// For scene objects, this will be the hierarchical path of the object using <paramref name="settings"/>.
    /// </summary>
    /// <param name="enrichmentConfiguration">Logger sink configuration.</param>
    /// <param name="settings">Supplies settings for generating the hierarchical path of scene objects.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithUnityData(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        UnityLogEnricherSettings settings
    ) =>
        enrichmentConfiguration == null ? throw new ArgumentNullException(nameof(enrichmentConfiguration))
        : settings == null ? throw new ArgumentNullException(nameof(settings))
        : enrichmentConfiguration.With(new UnityLogEnricher(settings));
}
