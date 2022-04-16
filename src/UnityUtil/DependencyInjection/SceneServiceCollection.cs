using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine.DependencyInjection;

[Serializable]
public class InspectorService
{
    [Required]
    public Object? Instance;

    [Tooltip(
        "Optional. All services are associated with a System.Type. This Type can be any Type in the service's inheritance hierarchy. " +
        "For example, a service component derived from Monobehaviour could be associated with its actual declared Type, " +
        "with Monobehaviour, or with UnityEngine.Object. The actual declared Type is assumed if you leave this field blank."
    )]
    public string TypeName = "";

    [HideInInspector, NonSerialized]
    public string Tag = "";
}

public class SceneServiceCollection : MonoBehaviour, IEnumerable<InspectorService>
{
    [Tooltip(
        "The service collection from which dependencies will be resolved. Order does not matter. " +
        "Note also that runtime changes to this collection will not have any affect.\n\n" +
        $"If there are multiple {nameof(SceneServiceCollection)} instances present in the scene, " +
        $"or multiple scenes with a {nameof(SceneServiceCollection)} are loaded at the same time, " +
        $"then their {nameof(Services)} will be combined. " +
        "This allows a game to dynamically register and unregister a scene's services at runtime. " +
        $"Note, however, that an error will result if multiple {nameof(SceneServiceCollection)} instances " +
        "try to register a service with the same parameters. In this case, it may be better to create a 'base' scene " +
        "with all common services, so that they are each registered once, or register the services with different tags."
    )]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = false)]
    public InspectorService[] Services = Array.Empty<InspectorService>();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake()
    {
        for (int s = 0; s < Services.Length; ++s) {
            InspectorService service = Services[s];
            DependencyInjector.Instance.RegisterService(service.TypeName, service.Instance!, gameObject.scene);
        }
    }

    public IEnumerator<InspectorService> GetEnumerator() => ((IEnumerable<InspectorService>)Services).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Services.GetEnumerator();

    /// <summary>
    /// Unregisters all services from this collection and any others in the scene.
    /// There should only be one collection per scene anyway.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnDestroy() => DependencyInjector.Instance.UnregisterSceneServices(gameObject.scene);

}
