using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityEngine {

    [Serializable]
    public struct Service {
        #pragma warning disable CA2235 // Mark all non-serializable fields

        [Tooltip("Optional. All services are associated with a System.Type. This Type can be any Type in the service's inheritance hierarchy. For example, a service component derived from Monobehaviour could be associated with its actual declared Type, with Monobehaviour, or with UnityEngine.Object. The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        [HideInInspector, NonSerialized]
        public string Tag;
        public MonoBehaviour Instance;

        #pragma warning restore CA2235 // Mark all non-serializable fields
    }

    public class DependencyInjector : MonoBehaviour {

        private static readonly ICollection<int> s_registeredScenes = new HashSet<int>();
        private static readonly IDictionary<Type, IDictionary<string, Service>> s_services = new Dictionary<Type, IDictionary<string, Service>>();

        private static ILogger s_logger;

        [Tooltip("The service collection from which dependencies will be resolved. Order does not matter. If there are multiple " + nameof(DependencyInjector) + " instances present in the scene, or multiple scenes with a " + nameof(DependencyInjector) + " are loaded at the same time, then their " + nameof(ServiceCollection) + "s will be combined. This allows a game to dynamically register and unregister a scene's services at runtime. Note, however, that an error will result if multiple " + nameof(DependencyInjector) + " instances try to register a service with the same parameters. In this case, it may be better to create a 'base' scene with all common services, so that they are each registered once, or register the services with different tags.")]
        public Service[] ServiceCollection;

        public const string InjectMethodName = "Inject";

        /// <summary>
        /// Inject all dependencies into the specified client.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="client">A client with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(object client) => ResolveDependenciesOf(new[] { client });
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(IEnumerable<object> clients) {
            foreach (object client in clients) {

                // Ensure that the necessary services have been registered to resolve this dependency
                // For GameObject clients, these are all services added to all DependencyInjector components in the same scene
                // For non-GameObject clients, these are all services added to all DependencyInjector components in all loaded scenes
                if (client is GameObject clientObj)
                    ensureServicesRegistered(clientObj.scene);
                else
                {
                    for (int s = 0; s < SceneManager.sceneCount; ++s)
                        ensureServicesRegistered(SceneManager.GetSceneAt(s));
                }

                // Resolve dependencies by calling every Inject method in the client's inheritance hierarchy
                MethodInfo[] injectMethods = client.GetType().GetMethods().Where(m => m.Name == InjectMethodName).ToArray();
                for (int m = 0; m < injectMethods.Length; ++m)
                    inject(injectMethods[m], client);

            }
        }

        // EVENT HANDLERS
        private static void ensureServicesRegistered(Scene scene)
        {
            if (s_registeredScenes.Contains(scene.buildIndex))
                return;

            DependencyInjector[] dependencyInjectors = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<DependencyInjector>()).ToArray();
            if (dependencyInjectors.Length == 0)
            {
                Debug.LogWarning($"No {nameof(DependencyInjector)} present in scene '{scene.path}'. No services will be loaded.");
                return;
            }
            else
            {
                if (dependencyInjectors.Length > 1)
                    Debug.LogWarning($"More than one {nameof(DependencyInjector)} present in scene '{scene.path}'. For simplicity, consider maintaining one {nameof(DependencyInjector)} per scene, and pull services shared by multiple scenes into a separate scene."); ;
                for (int i = 0; i < dependencyInjectors.Length; ++i)
                    registerServicesOf(dependencyInjectors[i]);
                s_registeredScenes.Add(scene.buildIndex);
            }
        }
        private static void registerServicesOf(DependencyInjector dependencyInjector) {
            Service[] services = dependencyInjector.ServiceCollection;

            // Update every service's Type/Tag
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            for (int s = 0; s < services.Length; ++s) {
                Service service = services[s];

                if (string.IsNullOrEmpty(service.TypeName))
                    service.TypeName = service.Instance.GetType().AssemblyQualifiedName;
                service.Tag = service.Instance.tag;
                services[s] = service;
            }

            // Get or set the logger that we will use for our own logging
            if (s_logger == null)
            {
                Service[] loggerProviderServices = services
                    .Where(s => typeof(ILoggerProvider).AssemblyQualifiedName.Contains(s.TypeName))
                    .ToArray();
                if (loggerProviderServices.Length == 0)
                {
                    s_logger = Debug.unityLogger;
                    Debug.LogWarning($"No {nameof(ILoggerProvider)} configured in service collection. {dependencyInjector.GetHierarchyNameWithType()} will use {nameof(Debug)}.{nameof(Debug.unityLogger)} for its own logging instead.");
                }
                else
                {
                    Service service = loggerProviderServices[0];
                    s_logger = (service.Instance as ILoggerProvider).GetLogger(dependencyInjector);
                    if (loggerProviderServices.Length > 1)
                        log(LogType.Warning, $"Configured multiple services with Type '{typeof(ILoggerProvider).FullName}'. {dependencyInjector.GetHierarchyNameWithType()} will use the first one (tag '{service.Tag}') for its own logging.");
                }
            }

            // Add every service specified in the Inspector to the private service collection
            log(LogType.Log, $"Awaking in scene '{SceneManager.GetActiveScene().path}', adding services...");
            for (int s = 0; s < services.Length; ++s) {
                Service service = services[s];

                // Get the service's Type, if it is valid
                Type type;
                try {
                    type = Type.GetType(service.TypeName);
                    if (type == null)
                        throw new InvalidOperationException($"Could not load Type '{service.TypeName}'.  Make sure that you provided its assembly-qualified name and that its assembly is loaded.");
                    if (!type.IsAssignableFrom(service.Instance.GetType()))
                        throw new InvalidOperationException($"The service instance configured for Type '{service.TypeName}' is not actually derived from that Type!");
                }
                catch (Exception ex) {
                    log(LogType.Error, $"Could not configure service of Type '{service.TypeName}': {ex.Message}");
                    continue;
                }

                // Add the service to the service collection, throwing an error if it's Type/Tag have already been configured
                bool typeAdded = s_services.TryGetValue(type, out IDictionary<string, Service> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(service.Tag, out _);
                    if (tagAdded) {
                        log(LogType.Error, $"Configured multiple services with Type '{service.TypeName}' and tag '{service.Tag}'");
                        continue;
                    }
                    else {
                        typedServices.Add(service.Tag, service);
                        log(LogType.Error, $"Successfully configured service of type '{service.TypeName}' and tag '{service.Tag}'.");
                    }
                }
                else
                    s_services.Add(type, new Dictionary<string, Service> { { service.Tag, service } });
            }
        }
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void OnDestroy() {
            // Assume that we are only being destroyed if the parent scene is being unloaded. Thus, that scene can be forgotten
            // If there are multiple DependencyInjector instances in same scene, all services in that scene will be removed after the first one is destroyed
            if (!s_registeredScenes.Contains(gameObject.scene.buildIndex))
                return;

            // Remove every service specified in the Inspector from the private service collection
            log(LogType.Log, $"Being destroyed, removing services...");
            int numSuccesses = 0;
            for (int s = 0; s < ServiceCollection.Length; ++s) {
                Service service = ServiceCollection[s];

                // Get the service's Type, if it is valid
                Type type;
                if (string.IsNullOrEmpty(service.TypeName))
                    type = service.Instance.GetType();
                else {
                    try {
                        type = Type.GetType(service.TypeName);
                    }
                    catch (Exception ex) {
                        log(LogType.Error, $"Could not remove service of Type '{service.TypeName}': {ex.Message}");
                        continue;
                    }
                }

                // Remove the service from the service collection
                bool typeAdded = s_services.TryGetValue(type, out IDictionary<string, Service> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(service.Tag, out _);
                    if (tagAdded) {
                        typedServices.Remove(service.Tag);
                        if (typedServices.Count == 0)
                            s_services.Remove(type);
                        ++numSuccesses;
                    }
                    else {
                        log(LogType.Error, $"Couldn't remove service with Type '{type}' and tag '{service.Tag}' because somehow it wasn't present in the service collection!");
                        continue;
                    }
                }
                else {
                    log(LogType.Error, $"Couldn't remove service of Type '{type}' because somehow it wasn't present in the service collection!");
                    continue;
                }
            }

            s_registeredScenes.Remove(gameObject.scene.buildIndex);

            // Log whether or not all services were removed successfully
            if (numSuccesses == ServiceCollection.Length)
                log(LogType.Log, $"Successfully removed all {ServiceCollection.Length} services.");
            else
                log(LogType.Error, $"Only removed {numSuccesses} out of {ServiceCollection.Length} registered services.");
        }

        private static void log(LogType logType, object message) => s_logger?.Log(logType, message);
        private static void inject(MethodInfo injectMethod, object client) {
            var injectedTypes = new HashSet<Type>();

            ParameterInfo[] parameters = injectMethod.GetParameters();
            var services = new Object[parameters.Length];
            for (int p = 0; p < parameters.Length; ++p) {
                ParameterInfo param = parameters[p];
                Type pType = param.ParameterType;
                string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as Object)?.name ?? $"{client.GetType().FullName} instance";

                // Warn if a dependency with this Type has already been injected
                bool firstInjection = injectedTypes.Add(pType);
                if (!firstInjection)
                    log(LogType.Warning, $"{clientName} has multiple dependencies of Type '{pType.FullName}'.");

                // If this dependency can't be resolved, then skip it with an error and clear the field
                bool resolved = s_services.TryGetValue(pType, out IDictionary<string, Service> typedServices);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{pType.FullName}', but no service was registered with that Type. Did you forget to add a service to the service collection?");
                    continue;
                }
                InjectTagAttribute injAttr = param.GetCustomAttribute<InjectTagAttribute>();
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? "Untagged" : injAttr.Tag;
                resolved = typedServices.TryGetValue(tag, out Service service);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{pType.FullName}' with tag '{tag}', but no matching service was registered. Did you forget to tag a service?");
                    continue;
                }

                // Log that this dependency has been resolved
                services[p] = service.Instance;
                log(LogType.Log, $"{clientName} had dependency of Type '{pType.FullName}'{(untagged ? "" : $" with tag '{tag}'")} injected into field '{param.Name}'.");
            }

            injectMethod.Invoke(client, services);
        }

    }

}
