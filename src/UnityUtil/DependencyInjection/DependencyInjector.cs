using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityEngine.DependencyInjection
{
    public class DependencyResolutionCounts
    {
        public DependencyResolutionCounts(IReadOnlyDictionary<Type, int> cachedResolutionCounts, IReadOnlyDictionary<Type, int> uncachedResolutionCounts)
        {
            Cached = cachedResolutionCounts;
            Uncached = uncachedResolutionCounts;
        }
        public IReadOnlyDictionary<Type, int> Cached { get; }
        public IReadOnlyDictionary<Type, int> Uncached { get; }
    }

    internal readonly struct Service
    {
        public Service(Type serviceType, string tag, object instance)
        {
            ServiceType = serviceType;
            Tag = tag;
            Instance = instance;
        }
        public readonly Type ServiceType;
        public readonly string Tag;
        public readonly object Instance;
    }

    public class DependencyInjector
    {
        public const string DefaultTag = "Untagged";
        public const string InjectMethodName = "Inject";
        public const string DefaultLoggerProviderName = "default-logger-provider";
        public const BindingFlags InjectMethodBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        private const int DEFAULT_SCENE_HANDLE = -1;

        public static readonly DependencyInjector Instance = new(Array.Empty<Type>()) { RecordingResolutions = Device.Application.isEditor };

        private ILogger _logger = Debug.unityLogger;
        private ITypeMetadataProvider? _typeMetadataProvider;

        private readonly Dictionary<int, Dictionary<Type, Dictionary<string, Service>>> _services = new();

        /// <summary>
        /// This collection is only a field (rather than a local var) so as to reduce allocations in <see cref="loadDependeciesOfInjectMethod(object, MethodInfo)"/>
        /// </summary>
        private readonly HashSet<(Type type, string? tag)> _injectedTypes = new();

        private bool _recording;
        private readonly HashSet<Type> _cachedResolutionTypes;
        private readonly IDictionary<Type, List<Action<object>>> _compiledInject = new Dictionary<Type, List<Action<object>>>();
        private readonly Dictionary<Type, int> _uncachedResolutionCounts = new();
        private readonly Dictionary<Type, int> _cachedResolutionCounts = new();

        #region Constructors/initialization

        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR. It exists purely for unit testing
        /// </summary>
        internal DependencyInjector(IEnumerable<Type> cachedResolutionTypes)
        {
            _cachedResolutionTypes = new HashSet<Type>(cachedResolutionTypes);
        }

        public bool Initialized { get; private set; }
        public void Initialize(ILoggerProvider loggerProvider) => Initialize(loggerProvider, new TypeMetadataProvider());
        internal void Initialize(ILoggerProvider loggerProvider, ITypeMetadataProvider typeMetadataProvider)
        {
            if (Initialized)
                throw new InvalidOperationException($"Cannot initialize a {nameof(DependencyInjector)} multiple times!");

            _typeMetadataProvider = typeMetadataProvider;
            _logger = loggerProvider.GetLogger(this);

            Initialized = true;
        }
        private void throwIfUninitialized(string methodName)
        {
            if (!Initialized)
                throw new InvalidOperationException($"Must call {nameof(Initialize)}() on a {nameof(DependencyInjector)} before calling its '{methodName}' method.");
        }

        #endregion

        #region Service registration

        public void RegisterService(string serviceTypeName, object instance, Scene? scene = null)
        {
            if (string.IsNullOrEmpty(serviceTypeName))
                serviceTypeName = instance.GetType().AssemblyQualifiedName;

            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType is null)
                throw new InvalidOperationException($"Could not load Type '{serviceTypeName}'. Make sure that you provided its assembly-qualified name and that its assembly is loaded.");
            if (!serviceType.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException($"The service instance registered for Type '{serviceTypeName}' is not actually derived from that Type!");

            RegisterService(serviceType, instance, scene);
        }
        public void RegisterService<TInstance>(TInstance instance, Scene? scene = null) where TInstance : class => RegisterService(typeof(TInstance), instance, scene);
        public void RegisterService<TService, TInstance>(TInstance instance, Scene? scene = null) where TInstance : class, TService => RegisterService(typeof(TService), instance, scene);

        /// <summary>
        /// Register <paramref name="service"/> present in <paramref name="scene"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// A <see cref="Service"/> with the provided <see cref="Service.ServiceType"/> and <see cref="Service.Tag"/> has already been registered.
        /// </exception>
        public void RegisterService(Type serviceType, object instance, Scene? scene = null)
        {
            throwIfUninitialized(nameof(RegisterService));

            var service = new Service(serviceType, (instance as Component)?.tag ?? DefaultTag, instance);

            // Check if the provided service is for logging
            if (serviceType == typeof(ILoggerProvider) && _logger == null)
                _logger = ((ILoggerProvider)instance).GetLogger(this);
            else if (serviceType == typeof(ILogger) && _logger == null)
                _logger = (ILogger)instance;


            // Register this service with the provided scene (if one was provided), so that it can be unloaded later if the scene is unloaded
            // Show an error if provided service's type/tag match those of an already registered service
#pragma warning disable IDE0008 // Use explicit type
            int sceneHandle = scene.HasValue ? scene.Value.handle : DEFAULT_SCENE_HANDLE;
            bool sceneAdded = _services.TryGetValue(sceneHandle, out var sceneServices);
            if (!sceneAdded) {
                sceneServices = new Dictionary<Type, Dictionary<string, Service>>();
                _services.Add(sceneHandle, sceneServices);
            }

            bool typeAdded = sceneServices.TryGetValue(serviceType, out var typedServices);
            if (!typeAdded) {
                typedServices = new Dictionary<string, Service>();
                sceneServices.Add(serviceType, typedServices);
            }
#pragma warning restore IDE0008 // Use explicit type

            bool tagAdded = typedServices.ContainsKey(service.Tag);
            string fromSceneMsg = scene.HasValue ? $" from scene '{scene.Value.name}'" : "";
            if (tagAdded)
                throw new InvalidOperationException($"Attempt to register multiple services with Type '{service.ServiceType.Name}' and tag '{service.Tag}'{fromSceneMsg}");
            else {
                typedServices.Add(service.Tag, service);
                _logger.Log($"Successfully registered service of type '{service.ServiceType.Name}' and tag '{service.Tag}'{fromSceneMsg}");
            }
        }

        #endregion

        #region Resolving client dependencies


        /// <summary>
        /// <para>
        /// Use these rules to cache commonly resolved dependencies, speeding up Scene load times.
        /// We use this whitelist approach because caching ALL dependency resolutions could use up significant memory, and could actually
        /// worsen performance if many of the dependencies were only to be resolved by one client.
        /// </para>
        /// <para>
        /// After a class instance with one of these types has had its dependences resolved via reflection,
        /// the reflected metadata and matching services will be cached, so that
        /// subsequent clients of the same type will have their dependencies injected much faster.
        /// This is useful if you know you will have many client components in a scene with the same type.
        /// </para>
        /// </summary>
        public IReadOnlyCollection<Type> CachedResolutionTypes => _cachedResolutionTypes;

        /// <summary>
        /// Toggles recording how many times service <see cref="Type"/>s are resolved at runtime, for optimization purposes.
        /// </summary>
        public bool RecordingResolutions {
            get => _recording;
            set {
                if (_recording == value)
                    return;

                _recording = value;
                if (!_recording) {
                    _cachedResolutionCounts.Clear();
                    _uncachedResolutionCounts.Clear();
                }

                _logger?.Log($"{(_recording ? "Started" : "Stopped")} recording dependency resolutions");
            }
        }

        public void CacheResolution(Type clientType) => _cachedResolutionTypes.Add(clientType);

        /// <summary>
        /// Gets/sets the number of times that each service <see cref="Type"/> has been resolved at runtime.
        /// </summary>
        public DependencyResolutionCounts ServiceResolutionCounts => new(
            cachedResolutionCounts: _cachedResolutionCounts,
            uncachedResolutionCounts: _uncachedResolutionCounts
        );

        /// <summary>
        /// Inject all dependencies into the specified client.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="client">A client with service dependencies that need to be resolved.</param>
        public void ResolveDependenciesOf(object client)
        {
            throwIfUninitialized(nameof(ResolveDependenciesOf));

            // Resolve dependencies by calling every Inject method in the client's inheritance hierarchy.
            // If the client's type or any of its inherited types have cached inject methods,
            // then use/compile those as necessary so that injection is faster for future clients with these types.
            Type serviceType = client.GetType();
            Type objectType = typeof(object);
            Type? cachedParentType = null;
            List<Action<object>>? compiledInjectList = null;     // Will only be initialized if this client's type or one of its parent types is cached, to save heap allocations
            do {
                // Use compiled inject methods, if they exist
                if (_compiledInject.TryGetValue(serviceType, out List<Action<object>> compiledInjectMethods)) {
                    for (int m = 0; m < compiledInjectMethods.Count; ++m)
                        compiledInjectMethods[m](client);
                    if (_recording)
                        _cachedResolutionCounts[serviceType] = _cachedResolutionCounts.TryGetValue(serviceType, out int count) ? count + 1 : 1;
                    return;
                }

                // Get the inject method on this type (will throw if more than one method matches)
                MethodInfo injectMethod = _typeMetadataProvider!.GetMethod(serviceType, InjectMethodName, InjectMethodBindingFlags);
                if (injectMethod is null)
                    goto ContinueHierarchy;

                string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as Object)?.name ?? $"{injectMethod.DeclaringType.FullName} instance";
                object[] dependencies = getDependeciesOfInjectMethod(clientName, injectMethod);
                if (dependencies.Length == 0)
                    goto ContinueHierarchy;

                // Check if the inject method should be compiled. If so, compile/call it; otherwise, invoke it via reflection
                bool compile = true;
                if (cachedParentType is null) {
                    if (_cachedResolutionTypes.Contains(serviceType))
                        cachedParentType = serviceType;
                    else {
                        compile = false;
                        injectMethod.Invoke(client, dependencies);
                        if (_recording)
                            _uncachedResolutionCounts[serviceType] = _uncachedResolutionCounts.TryGetValue(serviceType, out int count) ? count + 1 : 1;
                    }
                }
                if (compile) {
                    string compiledMethodName = $"{nameof(ResolveDependenciesOf)}_{injectMethod.DeclaringType.Name}_Generated";
                    Action<object> compiledInject = _typeMetadataProvider.CompileMethodCall(compiledMethodName, nameof(client), injectMethod, dependencies);
                    (compiledInjectList ??= new List<Action<object>>()).Add(compiledInject);
                    compiledInject(client);
                    if (_recording)
                        _cachedResolutionCounts[serviceType] = 1;
                }

                ContinueHierarchy:
                serviceType = serviceType.BaseType;
            } while (serviceType != objectType && serviceType is not null);

            if (cachedParentType is not null)
                _compiledInject.Add(cachedParentType, compiledInjectList!);
        }

        public void UnregisterSceneServices(Scene scene)
        {
            throwIfUninitialized(nameof(UnregisterSceneServices));

            if (!_services.ContainsKey(scene.handle)) {
                _logger.LogWarning($"Cannot unregister services from scene '{scene.name}', as none have been registered. Are you trying to destroy multiple service collections from the same scene?");
                return;
            }

            _logger.Log($"Unregistering services from scene '{scene.name}'...");
            int numSceneServices = _services[scene.handle].Sum(x => x.Value.Values.Count);
            _services.Remove(scene.handle);
            _logger.Log($"Successfully unregistered all {numSceneServices} services from scene '{scene.name}'.");
        }

        /// <summary>
        /// Resolve the dependencies of <paramref name="injectMethod"/>.
        /// I.e., get the service that satisfies the <see cref="Type"/> and (optional) tag of each of <paramref name="injectMethod"/>'s parameters.
        /// </summary>
        /// <param name="clientName">Name of the client object instance on which <paramref name="injectMethod"/> can be called</param>
        /// <param name="injectMethod">The method for which to resolve dependencies</param>
        /// <returns>The dependencies (parameters) required by <paramref name="injectMethod"/></returns>
        private object[] getDependeciesOfInjectMethod(string clientName, MethodInfo injectMethod)
        {
            _injectedTypes.Clear();
            ParameterInfo[] parameters = _typeMetadataProvider!.GetMethodParameters(injectMethod);
            object[] dependencies = new object[parameters.Length];
            for (int p = 0; p < parameters.Length; ++p) {
                Type paramType = parameters[p].ParameterType;

                // Get the dependency's requested tag, if it exists
                InjectTagAttribute? injAttr = _typeMetadataProvider!.GetCustomAttribute<InjectTagAttribute>(parameters[p]);
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? DefaultTag : injAttr!.Tag;

                // Warn if a dependency with this Type and tag has already been injected
                bool firstInjection = _injectedTypes.Add((paramType, tag));
                if (!firstInjection)
                    _logger.LogWarning($"{clientName} has multiple dependencies of Type '{paramType.FullName}' with tag '{tag}'.");

                TryGetService(paramType, tag, clientName, out Service service);
                dependencies[p] = service.Instance;

                _logger.Log($"{clientName} had dependency of Type '{paramType.FullName}'{(untagged ? "" : $" with tag '{tag}'")} injected into parameter '{parameters[p].Name}'.");
            }

            return dependencies;
        }
        internal void TryGetService(Type serviceType, string tag, string clientName, out Service service)
        {
            Dictionary<string, Service>? typedServices = null;
            foreach (int scene in _services.Keys) {
                if (_services[scene].TryGetValue(serviceType, out typedServices))
                    break;
            }
            if (typedServices is null)
                throw new KeyNotFoundException($"{clientName} has a dependency of Type '{serviceType.FullName}', but no service was registered with that Type. Did you forget to add a service to the service collection?");

            bool resolved = typedServices.TryGetValue(tag, out service);
            if (!resolved)
                throw new KeyNotFoundException($"{clientName} has a dependency of Type '{serviceType.FullName}' with tag '{tag}', but no matching service was registered. Did you forget to tag a service?");
        }

        #endregion

    }

}
