using Serilog.Core;
using Serilog.Events;
using System.Text;
using UnityEngine;

namespace Serilog.Enrichers.UnityObjectPath;

public class UnityObjectPathLogEnricher : ILogEventEnricher
{
    private readonly UnityObjectPathLogEnricherSettings _unityObjectPathLogEnricherSettings;

    /// <summary>
    /// Purposefully collides with the key used by <a href="https://github.com/KuraiAndras/Serilog.Sinks.Unity3D/blob/master/Serilog.Sinks.Unity3D/Assets/Serilog.Sinks.Unity3D/UnityObjectEnricher.cs">Serilog.Sinks.Unity3D's <c>UnityObjectEnricher</c></a>.
    /// Ideally, that enricher (or an app-specific enricher) will add the Unity context object,
    /// then this enricher will use it to construct the context's "path".
    /// </summary>
    public const string UnityContextKey = "%_DO_NOT_USE_UNITY_ID_DO_NOT_USE%";

    public const string UnityPathKey = "UnityObjectPath";

    public UnityObjectPathLogEnricher(UnityObjectPathLogEnricherSettings unityObjectPathLogEnricherSettings)
    {
        _unityObjectPathLogEnricherSettings = unityObjectPathLogEnricherSettings;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (
            !logEvent.Properties.TryGetValue(UnityContextKey, out LogEventPropertyValue? contextPropertyValue)
            || contextPropertyValue is not ScalarValue contextScalarValue
            || contextScalarValue.Value is not Object unityContext
        ) {
            return;
        }

        LogEventProperty logEventProperty = new(UnityPathKey, new ScalarValue(getUnityObjectPath(unityContext)));
        logEvent.AddPropertyIfAbsent(logEventProperty);
    }

    private string getUnityObjectPath(Object context) =>
        context is not Component component
            ? context.name
            : getName(
                component.transform,
                _unityObjectPathLogEnricherSettings!.NumParents,
                _unityObjectPathLogEnricherSettings!.AncestorNameSeparator
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
