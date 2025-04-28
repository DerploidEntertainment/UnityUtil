using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Serilog.Enrichers.Unity;

/// <summary>
/// Enriches <see cref="LogEvent"/>s with information from the Unity engine.
/// </summary>
/// <param name="unityLogEnricherSettings"><inheritdoc cref="UnityLogEnricherSettings" path="/summary"/></param>
public class UnityLogEnricher(UnityLogEnricherSettings? unityLogEnricherSettings = null) : ILogEventEnricher
{
    private readonly UnityLogEnricherSettings _unityLogEnricherSettings = unityLogEnricherSettings ?? new UnityLogEnricherSettings();

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_unityLogEnricherSettings.WithFrameCount)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.FrameCountLogProperty, Time.frameCount));

        if (_unityLogEnricherSettings.WithTimeSinceLevelLoad)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeSinceLevelLoadLogProperty, Time.timeSinceLevelLoad));

        if (_unityLogEnricherSettings.WithTimeSinceLevelLoadAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeSinceLevelLoadAsDoubleLogProperty, Time.timeSinceLevelLoadAsDouble));

        if (_unityLogEnricherSettings.WithUnscaledTime)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.UnscaledTimeLogProperty, Time.unscaledTime));

        if (_unityLogEnricherSettings.WithUnscaledTimeAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.UnscaledTimeAsDoubleLogProperty, Time.unscaledTimeAsDouble));

        if (_unityLogEnricherSettings.WithTime)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeLogProperty, Time.time));

        if (_unityLogEnricherSettings.WithTimeAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeAsDoubleLogProperty, Time.timeAsDouble));
    }
}
