using Serilog.Core;
using Serilog.Events;
using System.Text;
using UnityEngine;

namespace Serilog.Enrichers.UnityObjectPath;

public class UnityLogEnricher(UnityLogEnricherSettings unityLogEnricherSettings) : ILogEventEnricher
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

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (unityLogEnricherSettings.IncludeFrameCount)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(FrameCountKey, new ScalarValue(Time.frameCount)));

        if (unityLogEnricherSettings.IncludeTimeSinceLevelLoad)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeSinceLevelLoadKey, new ScalarValue(Time.timeSinceLevelLoad)));

        if (unityLogEnricherSettings.IncludeTimeSinceLevelLoadAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeSinceLevelLoadAsDoubleKey, new ScalarValue(Time.timeSinceLevelLoadAsDouble)));

        if (unityLogEnricherSettings.IncludeUnscaledTime)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(UnscaledTimeKey, new ScalarValue(Time.unscaledTime)));

        if (unityLogEnricherSettings.IncludeUnscaledTimeAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(UnscaledTimeAsDoubleKey, new ScalarValue(Time.unscaledTimeAsDouble)));

        if (unityLogEnricherSettings.IncludeTime)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeKey, new ScalarValue(Time.time)));

        if (unityLogEnricherSettings.IncludeTimeAsDouble)
            logEvent.AddPropertyIfAbsent(new LogEventProperty(TimeAsDoubleKey, new ScalarValue(Time.timeAsDouble)));

        if (
            unityLogEnricherSettings.IncludeSceneObjectPath
            && logEvent.Properties.TryGetValue(UnityContextKey, out LogEventPropertyValue? contextPropertyValue)
            && contextPropertyValue is ScalarValue contextScalarValue
            && contextScalarValue.Value is Object unityContext
        )
            logEvent.AddPropertyIfAbsent(new LogEventProperty(ObjectPathKey, new ScalarValue(getUnityObjectPath(unityContext))));
    }

    private string getUnityObjectPath(Object context) =>
        context is not Component component
            ? context.name
            : getName(
                component.transform,
                unityLogEnricherSettings!.ParentCount,
                unityLogEnricherSettings!.ParentNameSeparator
            );

    private static string getName(Transform transform, uint numParents, string separator)
    {
        Transform trans = transform;
        var nameBuilder = new StringBuilder(trans.name);
        for (int p = 0; p < numParents; ++p) {
            trans = trans.parent;
            if (trans == null)
                break;
            nameBuilder.Insert(0, trans.name + separator);
        }

        return nameBuilder.ToString();
    }
}
