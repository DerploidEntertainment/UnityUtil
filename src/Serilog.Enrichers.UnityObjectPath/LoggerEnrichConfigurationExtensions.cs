using Serilog.Configuration;
using Serilog.Enrichers.UnityObjectPath;
using System;

namespace Serilog;

public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enriches log events with the "path" of the <see cref="UnityEngine.Object"/> context.
    /// For assets (e.g., <see cref="UnityEngine.ScriptableObject"/>s), this will simply be the <see cref="UnityEngine.Object.name"/>.
    /// For scene objects, this will be the hierarchical path of the object using <paramref name="settings"/>.
    /// </summary>
    /// <param name="enrichmentConfiguration">Logger sink configuration.</param>
    /// <param name="settings">Supplies settings for generating the hierarchical path of scene objects.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration UnityObjectPath(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        UnityObjectPathLogEnricherSettings settings
    )
    {
        if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        return enrichmentConfiguration.With(new UnityObjectPathLogEnricher(settings));
    }
}
