using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityEngine.DependencyInjection
{

    public class DependencyInjector {

        private class Service {
            public Type ServiceType;
            public string Tag;
            public object Instance;
        }

        public const string DefaultTag = "Untagged";
        public const string InjectMethodName = "Inject";
        public const string DefaultLoggerProviderName = "default-logger-provider";

        private const int DEFAULT_SCENE_HANDLE = -1;

        public static readonly DependencyInjector Instance = new DependencyInjector();

        private ILogger _logger = Debug.unityLogger;

        private bool _initialized = false;
        private readonly HashSet<Type> _cachedResolutionTypes = new HashSet<Type>();
        private readonly IDictionary<Type, Action<object>[]> _compiledInject = new Dictionary<Type, Action<object>[]>();
        private readonly Dictionary<int, Dictionary<Type, Dictionary<string, Service>>> _services =
            new Dictionary<int, Dictionary<Type, Dictionary<string, Service>>>();

        private readonly Dictionary<Type, int> _uncachedResolutionCounts = new Dictionary<Type, int>();
        private readonly Dictionary<Type, int> _cachedResolutionCounts = new Dictionary<Type, int>();

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
        public List<Type> CacheResolutionForTypes { get; } = new List<Type>();

        public void RegisterLoggerProvider(ILoggerProvider loggerProvider, Scene? scene = null)
        {
            _logger = loggerProvider.GetLogger(this);
            RegisterService(typeof(ILoggerProvider), loggerProvider, scene);
        }
        public void RegisterService(string serviceTypeName, object instance, Scene? scene = null)
        {
            if (string.IsNullOrEmpty(serviceTypeName))
                serviceTypeName = instance.GetType().AssemblyQualifiedName;

            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
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
            var service = new Service {
                Instance = instance,
                Tag = (instance as Component)?.tag ?? DefaultTag,
                ServiceType = serviceType,
            };

            // Check if the provided service is for logging
            if (serviceType == typeof(ILoggerProvider) && _logger == null)
                _logger = ((ILoggerProvider)instance).GetLogger(this);
            else if (serviceType == typeof(ILogger) && _logger == null)
                _logger = (ILogger)instance;


            // Register this service with the provided scene (if one was provided), so that it can be unloaded later if the scene is unloaded
            // Show an error if provided service's type/tag match those of an already registered service
            // (show, not throw, so that error messages will be shown for ALL duplicate registrations, not just the first one encountered)
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

            bool tagAdded = typedServices.ContainsKey(service.Tag);
            string fromSceneMsg = scene.HasValue ? $" from scene '{scene.Value.name}'" : "";
            if (tagAdded)
                _logger.LogError($"Attempt to register multiple services with Type '{service.ServiceType.Name}' and tag '{service.Tag}'{fromSceneMsg}");
            else {
                typedServices.Add(service.Tag, service);
                _logger.Log($"Successfully registered service of type '{service.ServiceType.Name}' and tag '{service.Tag}'{fromSceneMsg}");
            }
        }

        /// <summary>
        /// Inject all dependencies into the specified client.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="client">A client with service dependencies that need to be resolved.</param>
        public void ResolveDependenciesOf(object client)
        {
            if (!_initialized) {
                initialize();
                _initialized = true;
            }

            // Resolve dependencies by calling every Inject method in the client's inheritance hierarchy.
            // If the client's type or any of its inherited types have cached inject methods,
            // then use/compile those as necessary so that injection is faster for future clients with these types.
            Type serviceType = client.GetType();
            Type objectType = typeof(object);
            Type cachedParentType = null;
            var compiledInjectList = new List<Action<object>>();
            BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            do {
                // Use compiled inject methods, if they exist
                if (_compiledInject.TryGetValue(serviceType, out Action<object>[] compiledInjectMethods)) {
                    for (int m = 0; m < compiledInjectMethods.Length; ++m)
                        compiledInjectMethods[m](client);
                    ++_cachedResolutionCounts[serviceType];
                    return;
                }

                // If not, check if the inject method of the current type should be compiled
                // If so, compile/call it; otherwise, invoke it via reflection
                bool compile = true;
                MethodInfo injectMethod = serviceType.GetMethod(InjectMethodName, bindingFlags);
                if (injectMethod != null) {
                    object[] dependencies = getDependeciesOfInjectMethod(client, injectMethod);
                    if (cachedParentType == null) {
                        if (_cachedResolutionTypes.Contains(serviceType))
                            cachedParentType = serviceType;
                        else {
                            compile = false;
                            injectMethod.Invoke(client, dependencies);
                            _uncachedResolutionCounts[serviceType] = _uncachedResolutionCounts.TryGetValue(serviceType, out int count) ? count + 1 : 1;
                        }
                    }
                    if (compile) {
                        Action<object> compiledInject = compileInjectMethod(injectMethod, dependencies);
                        compiledInjectList.Add(compiledInject);
                        compiledInject(client);
                        _cachedResolutionCounts[serviceType] = 1;
                    }
                }

                serviceType = serviceType.BaseType;
            } while (serviceType != objectType);

            if (cachedParentType != null)
                _compiledInject.Add(cachedParentType, compiledInjectList.ToArray());


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
        public void ResolveDependenciesOf(IEnumerable<object> clients) {
            foreach (object client in clients)
                ResolveDependenciesOf(client);
        }

        public void UnregisterSceneServices(Scene scene) {
            if (!_services.ContainsKey(scene.handle)) {
                _logger.LogWarning($"Cannot unregister services from scene '{scene.name}', as none have been registered. Are you trying to destroy multiple service collections from the same scene?");
                return;
            }

            _logger.Log($"Unregistering services from scene '{scene.name}'...");
            int numSceneServices = _services[scene.handle].Sum(x => x.Value.Values.Count);
            _services.Remove(scene.handle);
            _logger.Log($"Successfully unregistered all {numSceneServices} services from scene '{scene.name}'.");
        }

        private void initialize()
        {
            for (int t = 0; t < CacheResolutionForTypes.Count; ++t)
                _cachedResolutionTypes.Add(CacheResolutionForTypes[t]);
        }
        private object[] getDependeciesOfInjectMethod(object client, MethodInfo injectMethod)
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
                    _logger.LogWarning($"{clientName} has multiple dependencies of Type '{paramType.FullName}'.");

                // If this dependency can't be resolved, then skip it with an error message and clear the field
                // Error message is shown, not thrown, so that caller can see every dependency that's missing, not just the first one
                bool resolved = false;
                Dictionary<string, Service> typedServices = null;
                foreach (int scene in _services.Keys) {
                    if (_services[scene].TryGetValue(paramType, out typedServices)) {
                        resolved = true;
                        break;
                    }
                }
                if (!resolved) {
                    _logger.LogError($"{clientName} has a dependency of Type '{paramType.FullName}', but no service was registered with that Type. Did you forget to add a service to the service collection?");
                    continue;
                }
                InjectTagAttribute injAttr = parameters[p].GetCustomAttribute<InjectTagAttribute>();
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? DefaultTag : injAttr.Tag;
                resolved = typedServices.TryGetValue(tag, out Service service);
                if (!resolved) {
                    _logger.LogError( $"{clientName} has a dependency of Type '{paramType.FullName}' with tag '{tag}', but no matching service was registered. Did you forget to tag a service?");
                    continue;
                }

                // Log that this dependency has been resolved
                dependencies[p] = service.Instance;
                _logger.Log($"{clientName} had dependency of Type '{paramType.FullName}'{(untagged ? "" : $" with tag '{tag}'")} injected into parameter '{parameters[p].Name}'.");
            }

            return dependencies;
        }

    }

}
