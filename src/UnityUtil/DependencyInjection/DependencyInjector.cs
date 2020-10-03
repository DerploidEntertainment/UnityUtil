using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityEngine {

#pragma warning disable CA2235 // Mark all non-serializable fields

    [Serializable]
    public class InspectorService {

        public Object Instance;
        [Tooltip(
            "Optional. All services are associated with a System.Type. This Type can be any Type in the service's inheritance hierarchy. " +
            "For example, a service component derived from Monobehaviour could be associated with its actual declared Type, " +
            "with Monobehaviour, or with UnityEngine.Object. The actual declared Type is assumed if you leave this field blank."
        )]
        public string TypeName;
        [HideInInspector, NonSerialized]
        public string Tag;
    }

#pragma warning restore CA2235 // Mark all non-serializable fields

    public class DependencyInjector : MonoBehaviour {

        private class Service {
            public Type ServiceType;
            public string Tag;
            public object Instance;
        }

        public const string DefaultTag = "Untagged";
        public const string InjectMethodName = "Inject";

        private static ILogger s_logger;

        private static readonly ICollection<int> s_registeredScenes = new HashSet<int>();
        private static readonly HashSet<Type> s_cachedResolutionTypes = new HashSet<Type>();
        private static readonly IDictionary<Type, Action<object>[]> s_compiledInject = new Dictionary<Type, Action<object>[]>();
        private static readonly IDictionary<Type, IDictionary<string, Service>> s_services = new Dictionary<Type, IDictionary<string, Service>>();

        [Tooltip(
            "The service collection from which dependencies will be resolved. Order does not matter.\n\n" +
            "If there are multiple " + nameof(DependencyInjector) + " instances present in the scene, " +
            "or multiple scenes with a " + nameof(DependencyInjector) + " are loaded at the same time, " +
            "then their " + nameof(ServiceCollection) + "s will be combined. " +
            "This allows a game to dynamically register and unregister a scene's services at runtime. " +
            "Note, however, that an error will result if multiple " + nameof(DependencyInjector) + " instances " +
            "try to register a service with the same parameters. In this case, it may be better to create a 'base' scene " +
            "with all common services, so that they are each registered once, or register the services with different tags."
        )]
        [TableList(AlwaysExpanded = true, ShowIndexLabels = false)]
        public InspectorService[] ServiceCollection;

        [Tooltip(
            "Use these rules to cache commonly resolved dependencies, speeding up Scene load times. " +
            "We use this whitelist approach because caching ALL dependency resolutions could use up significant memory, and could actually " +
            "worsen performance if many of the dependencies were only to be resolved by one client.\n\n" +
            "After a class instance with one of these types has had its dependences resolved via reflection, " +
            "the reflected metadata and matching services will be cached, so that " +
            "subsequent clients of the same type will have their dependencies injected much faster. " +
            "This is useful if you know you will have many client components in a scene with the same type."
        )]
        public string[] CacheResolutionForTypes;

        public void AddService<TInstance>(TInstance instance) where TInstance : class => AddService<TInstance, TInstance>(instance);
        public void AddService<TService, TInstance>(TInstance instance) where TInstance : class, TService {
            Type serviceType = typeof(TService);
            var service = new Service {
                Instance = instance,
                Tag = (instance as Component)?.tag ?? DefaultTag,
                ServiceType = serviceType,
            };

            addService(service);
        }
        /// <summary>
        /// Add the service to the service collection, throwing an error if it's Type/Tag have already been registered
        /// </summary>
        private static void addService(Service service) {
            bool typeAdded = s_services.TryGetValue(service.ServiceType, out IDictionary<string, Service> typedServices);
            if (typeAdded) {
                bool tagAdded = typedServices.TryGetValue(service.Tag, out _);
                if (tagAdded) {
                    log(LogType.Error, $"Registered multiple services with Type '{service.ServiceType.Name}' and tag '{service.Tag}'");
                    return;
                }
                else {
                    typedServices.Add(service.Tag, service);
                    log(LogType.Log, $"Successfully registered service of type '{service.ServiceType.Name}' and tag '{service.Tag}'.");
                }
            }
            else
                s_services.Add(service.ServiceType, new Dictionary<string, Service> { { service.Tag, service } });
        }

        /// <summary>
        /// Inject all dependencies into the specified client.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="client">A client with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(object client)
        {
            // Ensure that the necessary services have been registered to resolve this dependency
            // For GameObject clients, these are all services added to all DependencyInjector components in the same scene
            // For non-GameObject clients, these are all services added to all DependencyInjector components in all loaded scenes
            if (client is GameObject clientObj)
                ensureServicesRegistered(clientObj.scene);
            else {
                for (int s = 0; s < SceneManager.sceneCount; ++s)
                    ensureServicesRegistered(SceneManager.GetSceneAt(s));
            }
            Type clientType = client.GetType();
            if (s_compiledInject.TryGetValue(clientType, out Action<object>[] compiledInject)) {
                for (int m = 0; m < compiledInject.Length; ++m)
                    compiledInject[m](client);
                return;
            }


            // Resolve dependencies by calling every Inject method in the client's inheritance hierarchy.
            // If the client is not also a service class (is not a singleton), then compile these reflected methods and cache them
            // so that injection is faster next time we receive a client of this Type
            bool cache = s_cachedResolutionTypes.Contains(clientType);
            MethodInfo[] injectMethods = clientType.GetMethods().Where(m => m.Name == InjectMethodName).ToArray();
            compiledInject = new Action<object>[injectMethods.Length];
            for (int m = 0; m < injectMethods.Length; ++m) {
                object[] dependencies = getDependeciesOfInjectMethod(client, injectMethods[m]);
                if (cache) {
                    compiledInject[m] = compileInjectMethod(injectMethods[m], dependencies);
                    compiledInject[m](client);
                }
                else
                    injectMethods[m].Invoke(client, dependencies);
            }
            if (cache)
                s_compiledInject.Add(clientType, compiledInject);


            static Action<object> compileInjectMethod(MethodInfo injectMethod, object[] dependencies)
            {
                ParameterExpression clientParam = Expression.Parameter(typeof(object), nameof(client));
                IEnumerable<Expression> dependencyArgs = injectMethod
                    .GetParameters()
                    .Select((param, p) => Expression.Constant(dependencies[p], param.ParameterType));
                return (Action<object>)Expression.Lambda(
                    body: Expression.Call(instance: Expression.Convert(clientParam, injectMethod.DeclaringType), injectMethod, dependencyArgs),
                    name: $"{nameof(ResolveDependenciesOf)}_{injectMethod.DeclaringType.Name}_Generated",
                    parameters: new[] { clientParam }
                ).Compile();
            }
        }
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(IEnumerable<object> clients) {
            foreach (object client in clients)
                ResolveDependenciesOf(client);
        }

        // EVENT HANDLERS
        private static void ensureServicesRegistered(Scene scene)
        {
            if (s_registeredScenes.Contains(scene.buildIndex))
                return;

            DependencyInjector[] dependencyInjectors = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<DependencyInjector>()).ToArray();
            if (dependencyInjectors.Length == 0) {
                Debug.LogWarning($"No {nameof(DependencyInjector)} present in scene '{scene.path}'. Only default services will be loaded.");
                if (s_logger == null)
                    registerDefaultLoggerProvider();
                return;
            }
            else {
                if (dependencyInjectors.Length > 1)
                    Debug.LogWarning($"More than one {nameof(DependencyInjector)} present in scene '{scene.path}'. For simplicity, consider maintaining one {nameof(DependencyInjector)} per scene, and pull services shared by multiple scenes into a separate scene."); ;
                for (int i = 0; i < dependencyInjectors.Length; ++i)
                    registerServicesOf(dependencyInjectors[i]);
                s_registeredScenes.Add(scene.buildIndex);
            }
        }
        private static void registerServicesOf(DependencyInjector dependencyInjector) {
            InspectorService[] services = dependencyInjector.ServiceCollection;

            // Update every service's Type/Tag
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            for (int s = 0; s < services.Length; ++s) {
                InspectorService service = services[s];

                if (string.IsNullOrEmpty(service.TypeName))
                    service.TypeName = service.Instance.GetType().AssemblyQualifiedName;
                service.Tag = (service.Instance as Component)?.tag ?? DefaultTag;
                services[s] = service;
            }

            // Get or set the logger that we will use for our own logging
            if (s_logger == null) {
                InspectorService[] loggerProviderServices = services
                    .Where(s => typeof(ILoggerProvider).AssemblyQualifiedName.Contains(s.TypeName))
                    .ToArray();
                if (loggerProviderServices.Length == 0)
                    registerDefaultLoggerProvider(dependencyInjector);
                else {
                    InspectorService service = loggerProviderServices[0];
                    s_logger = (service.Instance as ILoggerProvider).GetLogger(dependencyInjector);
                    if (loggerProviderServices.Length > 1)
                        log(LogType.Warning, $"Configured multiple services with Type '{typeof(ILoggerProvider).FullName}'. {dependencyInjector.GetHierarchyNameWithType()} will use the first one (tag '{service.Tag}') for its own logging.");
                }
            }

            // Add every service specified in the Inspector to the private service collection
            log(LogType.Log, $"Awaking in scene '{SceneManager.GetActiveScene().path}', adding services...");
            for (int s = 0; s < services.Length; ++s) {
                InspectorService service = services[s];

                // Get the service's Type, if it is valid
                Type serviceType;
                try {
                    serviceType = Type.GetType(service.TypeName);
                    if (serviceType == null)
                        throw new InvalidOperationException($"Could not load Type '{service.TypeName}'. Make sure that you provided its assembly-qualified name and that its assembly is loaded.");
                    if (!serviceType.IsAssignableFrom(service.Instance.GetType()))
                        throw new InvalidOperationException($"The service instance registered for Type '{service.TypeName}' is not actually derived from that Type!");
                }
                catch (Exception ex) {
                    log(LogType.Error, $"Could not register service of Type '{service.TypeName}': {ex.Message}");
                    continue;
                }

                addService(new Service {
                    Instance = service.Instance,
                    Tag = service.Tag,
                    ServiceType = serviceType,
                });
            }

            // Add all types with cached resolutions to the private collection of such types
            for (int t = 0; t < dependencyInjector.CacheResolutionForTypes.Length; ++t) {
                string typeName = dependencyInjector.CacheResolutionForTypes[t];
                var cacheResolutionType = Type.GetType(typeName);
                if (cacheResolutionType == null)
                    log(LogType.Warning, $"Can not cache dependency resolutions for Type '{typeName}'. Make sure that you provided the correct assembly-qualified name and that its assembly is loaded.");
                else
                    s_cachedResolutionTypes.Add(cacheResolutionType);
            }
        }

        private static void registerDefaultLoggerProvider(DependencyInjector dependencyInjector = null)
        {
            DebugLoggerProvider loggerProvider = new GameObject().AddComponent<DebugLoggerProvider>();
            if (dependencyInjector == null) {
                s_logger = Debug.unityLogger;
                s_logger.LogWarning($"No {nameof(ILoggerProvider)} registered in service collection. A default one has been registered instead for our own logging.");
            }
            else {
                s_logger = loggerProvider.GetLogger(dependencyInjector);
                s_logger.LogWarning($"No {nameof(ILoggerProvider)} registered in service collection. {dependencyInjector.GetHierarchyNameWithType()} will register a default one instead to do its own logging.", context: dependencyInjector);
            }
            addService(new Service {
                ServiceType = typeof(ILoggerProvider),
                Instance = loggerProvider,
                Tag = DefaultTag,
            });
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
                InspectorService service = ServiceCollection[s];

                // Get the service's Type, if it is valid
                Type serviceType;
                if (string.IsNullOrEmpty(service.TypeName))
                    serviceType = service.Instance.GetType();
                else {
                    try {
                        serviceType = Type.GetType(service.TypeName);
                    }
                    catch (Exception ex) {
                        log(LogType.Error, $"Could not remove service of Type '{service.TypeName}': {ex.Message}");
                        continue;
                    }
                }

                // Remove the service from the service collection
                bool typeAdded = s_services.TryGetValue(serviceType, out IDictionary<string, Service> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(service.Tag, out _);
                    if (tagAdded) {
                        typedServices.Remove(service.Tag);
                        if (typedServices.Count == 0) {
                            s_services.Remove(serviceType);
                            s_compiledInject.Remove(serviceType);
                        }
                        ++numSuccesses;
                    }
                    else {
                        log(LogType.Error, $"Service of Type '{service.TypeName}' with tag '{service.Tag}' was not removed because somehow it wasn't present in the service collection!");
                        continue;
                    }
                }
                else {
                    log(LogType.Error, $"Service of Type '{service.TypeName}' was not removed because somehow it wasn't present in the service collection!");
                    continue;
                }
            }

            s_registeredScenes.Remove(gameObject.scene.buildIndex);

            // Log whether or not all services were removed successfully
            if (numSuccesses == ServiceCollection.Length)
                log(LogType.Log, $"Successfully removed all {ServiceCollection.Length} services.");
            else
                log(LogType.Error, $"Only removed {numSuccesses} out of {ServiceCollection.Length} registered services.");

            // Removed types with cached dependency resolutions as well
            for (int t = 0; t < CacheResolutionForTypes.Length; ++t) {
                string typeName = CacheResolutionForTypes[t];
                var cacheResolutionType = Type.GetType(typeName);
                if (cacheResolutionType == null)
                    log(LogType.Warning, $"Could not remove dependency resolution cache rule for Type '{typeName}', because that is an invalid type name. Make sure that you provided the correct assembly-qualified name and that its assembly is loaded.");
                else
                    s_cachedResolutionTypes.Remove(cacheResolutionType);
            }
        }

        private static void log(LogType logType, object message) => s_logger?.Log(logType, message);
        private static object[] getDependeciesOfInjectMethod(object client, MethodInfo injectMethod)
        {
            var injectedTypes = new HashSet<Type>();
            ParameterInfo[] parameters = injectMethod.GetParameters();
            object[] dependencies = new object[parameters.Length];
            for (int p = 0; p < parameters.Length; ++p) {
                Type paramType = parameters[p].ParameterType;
                string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as Object)?.name ?? $"{injectMethod.DeclaringType.FullName} instance";

                // Warn if a dependency with this Type has already been injected
                bool firstInjection = injectedTypes.Add(paramType);
                if (!firstInjection)
                    log(LogType.Warning, $"{clientName} has multiple dependencies of Type '{paramType.FullName}'.");

                // If this dependency can't be resolved, then skip it with an error and clear the field
                bool resolved = s_services.TryGetValue(paramType, out IDictionary<string, Service> typedServices);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{paramType.FullName}', but no service was registered with that Type. Did you forget to add a service to the service collection?");
                    continue;
                }
                InjectTagAttribute injAttr = parameters[p].GetCustomAttribute<InjectTagAttribute>();
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? DefaultTag : injAttr.Tag;
                resolved = typedServices.TryGetValue(tag, out Service service);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{paramType.FullName}' with tag '{tag}', but no matching service was registered. Did you forget to tag a service?");
                    continue;
                }

                // Log that this dependency has been resolved
                dependencies[p] = service.Instance;
                log(LogType.Log, $"{clientName} had dependency of Type '{paramType.FullName}'{(untagged ? "" : $" with tag '{tag}'")} injected into parameter '{parameters[p].Name}'.");
            }

            return dependencies;
        }

    }

}
