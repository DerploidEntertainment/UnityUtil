using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Serilog.Enrichers.Unity;

/// <summary>
/// Enriches <see cref="LogEvent"/>s with information from the Unity engine.
/// </summary>
/// <param name="unityLogEnricherSettings"><inheritdoc cref="UnityLogEnricherSettings" path="/summary"/></param>
public class UnityLogEnricher(UnityLogEnricherSettings unityLogEnricherSettings) : ILogEventEnricher
{
    private readonly UnityLogEnricherSettings _unityLogEnricherSettings = unityLogEnricherSettings;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_unityLogEnricherSettings.AddFrameCount)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.FrameCountLogProperty, Time.frameCount));

        if (_unityLogEnricherSettings.AddTimeSinceLevelLoad)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeSinceLevelLoadLogProperty, Time.timeSinceLevelLoad));

        if (_unityLogEnricherSettings.AddTimeSinceLevelLoadAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeSinceLevelLoadAsDoubleLogProperty, Time.timeSinceLevelLoadAsDouble));

        if (_unityLogEnricherSettings.AddUnscaledTime)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.UnscaledTimeLogProperty, Time.unscaledTime));

        if (_unityLogEnricherSettings.AddUnscaledTimeAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.UnscaledTimeAsDoubleLogProperty, Time.unscaledTimeAsDouble));

        if (_unityLogEnricherSettings.AddTime)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeLogProperty, Time.time));

        if (_unityLogEnricherSettings.AddTimeAsDouble)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_unityLogEnricherSettings.TimeAsDoubleLogProperty, Time.timeAsDouble));
    }
}
