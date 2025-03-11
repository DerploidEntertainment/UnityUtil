using Serilog.Core;
using Serilog.Events;
using System;
using System.Text;
using UnityEngine;
using U = UnityEngine;

namespace Serilog.Enrichers.Unity;

public class UnityLogEnricher : ILogEventEnricher
{
    /// <summary>
    /// Purposefully collides with the key used by
    /// <a href="https://github.com/KuraiAndras/Serilog.Sinks.Unity3D/blob/master/Serilog.Sinks.Unity3D/Assets/Serilog.Sinks.Unity3D/UnityObjectEnricher.cs">Serilog.Sinks.Unity3D's <c>UnityObjectEnricher</c></a>.
    /// Ideally, that enricher (or an app-specific enricher) will add the Unity context object,
    /// then this enricher will use it to construct the context's "path".
    /// </summary>
    public static readonly string UnityContextKey = "%_DO_NOT_USE_UNITY_ID_DO_NOT_USE%";

    public static readonly string FrameCountKey = "UnityFrameCount";
    public static readonly string TimeSinceLevelLoadKey = "UnityTimeSinceLevelLoad";
    public static readonly string TimeSinceLevelLoadAsDoubleKey = "UnityTimeSinceLevelLoadAsDouble";
    public static readonly string UnscaledTimeKey = "UnityUnscaledTime";
    public static readonly string UnscaledTimeAsDoubleKey = "UnityUnscaledTimeAsDouble";
    public static readonly string TimeKey = "UnityTime";
    public static readonly string TimeAsDoubleKey = "UnityTimeAsDouble";
    public static readonly string ObjectPathKey = "UnityObjectPath";

    private readonly UnityLogEnricherSettings _unityLogEnricherSettings;

    public UnityLogEnricher(UnityLogEnricherSettings unityLogEnricherSettings)
    {
        if (unityLogEnricherSettings.ParentCount < 0)
            throw new ArgumentException($"{nameof(UnityLogEnricherSettings.ParentCount)} must be >= 0", nameof(unityLogEnricherSettings));

        _unityLogEnricherSettings = unityLogEnricherSettings;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_unityLogEnricherSettings.AddFrameCount)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(FrameCountKey, new ScalarValue(Time.frameCount)));

        if (_unityLogEnricherSettings.AddTimeSinceLevelLoad)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeSinceLevelLoadKey, new ScalarValue(Time.timeSinceLevelLoad)));

        if (_unityLogEnricherSettings.AddTimeSinceLevelLoadAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeSinceLevelLoadAsDoubleKey, new ScalarValue(Time.timeSinceLevelLoadAsDouble)));

        if (_unityLogEnricherSettings.AddUnscaledTime)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(UnscaledTimeKey, new ScalarValue(Time.unscaledTime)));

        if (_unityLogEnricherSettings.AddUnscaledTimeAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(UnscaledTimeAsDoubleKey, new ScalarValue(Time.unscaledTimeAsDouble)));

        if (_unityLogEnricherSettings.AddTime)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeKey, new ScalarValue(Time.time)));

        if (_unityLogEnricherSettings.AddTimeAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeAsDoubleKey, new ScalarValue(Time.timeAsDouble)));

        if (
            _unityLogEnricherSettings.IncludeSceneObjectPath
            && logEvent.Properties.TryGetValue(UnityContextKey, out LogEventPropertyValue? contextPropertyValue)
            && contextPropertyValue is ScalarValue contextScalarValue
            && contextScalarValue.Value is U.Object unityContext
        )
            logEvent.AddPropertyIfAbsent(new LogEventProperty(ObjectPathKey, new ScalarValue(getUnityObjectPath(unityContext))));
    }

    private string getUnityObjectPath(U.Object context) =>
        context is not Component component
            ? context.name
            : getName(
                component.transform,
                _unityLogEnricherSettings!.ParentCount,
                _unityLogEnricherSettings!.ParentNameSeparator
            );

    private static string getName(Transform transform, int parentCount, string separator)
    {
        Transform trans = transform;
        var nameBuilder = new StringBuilder(trans.name);
        for (int p = 0; p < parentCount; ++p) {
            trans = trans.parent;
            if (trans == null)
                break;
            nameBuilder.Insert(0, trans.name + separator);
        }

        return nameBuilder.ToString();
    }
}
