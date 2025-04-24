using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using U = UnityEngine;
using MEL = Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.DependencyInjection;

public class DependencyResolutionCounts(
    IReadOnlyDictionary<Type, int> cachedResolutionCounts,
    IReadOnlyDictionary<Type, int> uncachedResolutionCounts
) {
    public IReadOnlyDictionary<Type, int> Cached { get; } = cachedResolutionCounts;
    public IReadOnlyDictionary<Type, int> Uncached { get; } = uncachedResolutionCounts;
}

public class DependencyInjector : IDisposable
{
    public const string DefaultInjectTag = "Untagged";
    public const string InjectMethodName = "Inject";
    public const string DefaultLoggerProviderName = "default-logger-provider";
    public const BindingFlags InjectMethodBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

    private const int DEFAULT_SCENE_HANDLE = -1;

    public static readonly DependencyInjector Instance = new(cachedResolutionTypes: []) {
        RecordingResolutions = U.Device.Application.isEditor
    };

    public ILoggerFactory? LoggerFactory { get; private set; }
    private ITypeMetadataProvider? _typeMetadataProvider;
    private ILogger<DependencyInjector>? _logger;

    private readonly Dictionary<int, Dictionary<Type, Dictionary<string, Service>>> _services = [];

    /// <summary>
    /// This collection is only a field (rather than a local var) so as to reduce allocations in <see cref="getDependeciesOfMethod(string, MethodBase, ParameterInfo[])"/>
    /// </summary>
    private readonly HashSet<(Type type, string? tag)> _injectedTypes = [];

    private bool _recording;
    private readonly HashSet<Type> _cachedResolutionTypes;
    private readonly Dictionary<Type, List<Action<object>>> _compiledInject = [];
    private readonly Dictionary<Type, Func<object>> _compiledConstructors = [];
    private readonly Dictionary<Type, int> _uncachedResolutionCounts = [];
    private readonly Dictionary<Type, int> _cachedResolutionCounts = [];

    #region Constructors/initialization

    /// <summary>
    /// DO NOT USE THIS CONSTRUCTOR. It exists purely for unit testing
    /// </summary>
    internal DependencyInjector(IEnumerable<Type> cachedResolutionTypes) => _cachedResolutionTypes = [.. cachedResolutionTypes];

    public bool Initialized { get; private set; }
    public void Initialize(ILoggerFactory loggerFactory) => Initialize(loggerFactory, new TypeMetadataProvider());
    internal void Initialize(ILoggerFactory loggerFactory, ITypeMetadataProvider typeMetadataProvider)
    {
        if (Disposed)
            throw new InvalidOperationException($"Cannot initialize a disposed {nameof(DependencyInjector)}!");
        if (Initialized)
            throw new InvalidOperationException($"Cannot initialize a {nameof(DependencyInjector)} multiple times!");

        _typeMetadataProvider = typeMetadataProvider;
        LoggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DependencyInjector>();

        Initialized = true;
    }
    private void throwIfInvalidState(string methodName)
    {
        if (Disposed)
            throw new InvalidOperationException($"Cannot call {methodName}() on a disposed {nameof(DependencyInjector)}");
        if (!Initialized)
            throw new InvalidOperationException($"Must call {nameof(Initialize)}() on a {nameof(DependencyInjector)} before calling {methodName}()");
    }

    #endregion

    #region Service registration

    public void RegisterService(string serviceTypeName, object instance, Scene? scene = null, string injectTag = DefaultInjectTag)
    {
        if (string.IsNullOrEmpty(serviceTypeName))
            serviceTypeName = instance.GetType().AssemblyQualifiedName;

        Type serviceType = Type.GetType(serviceTypeName, throwOnError: true)
            ?? throw new InvalidOperationException($"Could not load Type '{serviceTypeName}'. Make sure that you provided its assembly-qualified name and that its assembly is loaded.");
        if (!serviceType.IsInstanceOfType(instance))
            throw new InvalidOperationException($"The service instance registered for Type '{serviceTypeName}' is not actually derived from that Type!");

        RegisterService(serviceType, instance, scene, injectTag);
    }
    public void RegisterService<TInstance>(TInstance instance, Scene? scene = null, string injectTag = DefaultInjectTag) where TInstance : class => RegisterService(typeof(TInstance), instance, scene, injectTag);
    public void RegisterService<TService, TInstance>(TInstance instance, Scene? scene = null, string injectTag = DefaultInjectTag) where TInstance : class, TService => RegisterService(typeof(TService), instance, scene, injectTag);
    public void RegisterService(Type serviceType, object instance, Scene? scene = null, string injectTag = DefaultInjectTag)
    {
        var service = new Service(serviceType, injectTag, instance);
        registerService(service, scene);
    }

    public void RegisterService(string serviceTypeName, Func<object> instanceFactory, Scene? scene = null, string injectTag = DefaultInjectTag)
    {
        if (string.IsNullOrEmpty(serviceTypeName))
            serviceTypeName = instanceFactory.GetType().AssemblyQualifiedName;

        Type serviceType = Type.GetType(serviceTypeName)
            ?? throw new InvalidOperationException($"Could not load Type '{serviceTypeName}'. Make sure that you provided its assembly-qualified name and that its assembly is loaded.");
        if (!serviceType.IsAssignableFrom(instanceFactory.GetType()))
            throw new InvalidOperationException($"The service instance registered for Type '{serviceTypeName}' is not actually derived from that Type!");

        RegisterService(serviceType, instanceFactory, scene, injectTag);
    }
    public void RegisterService<TInstance>(Func<TInstance> instanceFactory, Scene? scene = null, string injectTag = DefaultInjectTag) where TInstance : class => RegisterService(typeof(TInstance), instanceFactory, scene, injectTag);
    public void RegisterService<TService, TInstance>(Func<TInstance> instanceFactory, Scene? scene = null, string injectTag = DefaultInjectTag) where TInstance : class, TService => RegisterService(typeof(TService), instanceFactory, scene, injectTag);
    public void RegisterService(Type serviceType, Func<object> instanceFactory, Scene? scene = null, string injectTag = DefaultInjectTag)
    {
        var service = new Service(serviceType, injectTag, instanceFactory);
        registerService(service, scene);
    }

    /// <summary>
    /// Register <paramref name="service"/> present in <paramref name="scene"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A <see cref="Service"/> with the provided <see cref="Service.ServiceType"/> and <see cref="Service.InjectTag"/> has already been registered.
    /// </exception>
    private void registerService(Service service, Scene? scene = null)
    {
        throwIfInvalidState(nameof(RegisterService));

        // Check if the provided service is for logging
        if (service.ServiceType == typeof(ILoggerFactory) && LoggerFactory == null)
            LoggerFactory = (ILoggerFactory)service.Instance;

        // Register this service with the provided scene (if one was provided), so that it can be unloaded later if the scene is unloaded
        // Show an error if provided service's type/tag match those of an already registered service
#pragma warning disable IDE0008 // Use explicit type
        int sceneHandle = scene.HasValue ? scene.Value.handle : DEFAULT_SCENE_HANDLE;
        bool sceneAdded = _services.TryGetValue(sceneHandle, out var sceneServices);
        if (!sceneAdded) {
            sceneServices = [];
            _services.Add(sceneHandle, sceneServices);
        }

        bool typeAdded = sceneServices.TryGetValue(service.ServiceType, out var typedServices);
        if (!typeAdded) {
            typedServices = [];
            sceneServices.Add(service.ServiceType, typedServices);
        }
#pragma warning restore IDE0008 // Use explicit type

        bool tagAdded = typedServices.ContainsKey(service.InjectTag);
        if (tagAdded)
            throw new InvalidOperationException($"Attempt to register multiple services with Type '{service.ServiceType.Name}' and tag '{service.InjectTag}'{(scene.HasValue ? $" from scene '{scene.Value.name}'" : "")}");
        else {
            typedServices.Add(service.InjectTag, service);
            log_RegisteredService(service, scene);
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

            if (_logger is not null)
                log_ToggledRecordingDependencyResolution(_recording);
        }
    }

    public void CacheResolution(Type clientType) => _cachedResolutionTypes.Add(clientType);

    /// <summary>
    /// Gets the number of times that each service <see cref="Type"/> has been resolved at runtime.
    /// </summary>
    public DependencyResolutionCounts GetServiceResolutionCounts() => new(
        // Ensure that counts in this object remain even if field dictionaries are cleared
        cachedResolutionCounts: _cachedResolutionCounts.ToDictionary(x => x.Key, x => x.Value),
        uncachedResolutionCounts: _uncachedResolutionCounts.ToDictionary(x => x.Key, x => x.Value)
    );

    /// <summary>
    /// Attempts to construct an instance of <typeparamref name="T"/>, using the constructor with the most parameters that
    /// can all be resolved from registered services. If no constructors can have all parameters resolved, then <see langword="null"/> is returned.
    /// </summary>
    /// <typeparam name="T">Type of object to be constructed.</typeparam>
    /// <returns>The constructed instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Could not resolve all dependencies (parameters) of any public constructor on <typeparamref name="T"/>.</exception>
    public T Construct<T>() => (T)Construct(typeof(T));

    /// <summary>
    /// Attempts to construct an instance of <paramref name="clientType"/>, using the constructor with the most parameters that
    /// can all be resolved from registered services. If no constructors can have all parameters resolved, then <see langword="null"/> is returned.
    /// </summary>
    /// <returns>The constructed instance of <paramref name="clientType"/>.</returns>
    /// <exception cref="InvalidOperationException">Could not resolve all dependencies (parameters) of any public constructor on <paramref name="clientType"/>.</exception>
    public object Construct(Type clientType)
    {
        throwIfInvalidState(nameof(Construct));

        // Use compiled constructor, if it exists
        if (_compiledConstructors.TryGetValue(clientType, out Func<object> compiledConstructor)) {
            object client = compiledConstructor();
            if (_recording)
                _cachedResolutionCounts[clientType] = _cachedResolutionCounts.TryGetValue(clientType, out int count) ? count + 1 : 1;
            return client;
        }

        (ConstructorInfo constructor, ParameterInfo[] parameters)[] constructors = [..
            _typeMetadataProvider!.GetConstructors(clientType)
                .Select(x => (constructor: x, parameters: _typeMetadataProvider!.GetMethodParameters(x)))
                .OrderByDescending(x => x.parameters.Length),
        ];

        if (constructors.Length == 0)
            return Activator.CreateInstance(clientType);

        foreach ((ConstructorInfo constructor, ParameterInfo[] parameters) in constructors) {
            object[] dependencies;
            string clientName = $"{parameters.Length}-parameter constructor of Type {clientType.Name}";
            try { dependencies = getDependeciesOfMethod(clientName, constructor, parameters); }
            catch (KeyNotFoundException) { continue; }

            // Check if the constructor should be compiled. If so, compile/call it; otherwise, invoke it via reflection
            object instance;
            if (_cachedResolutionTypes.Contains(clientType)) {
                compiledConstructor = _typeMetadataProvider.CompileConstructorCall(constructor, dependencies);
                _compiledConstructors.Add(clientType, compiledConstructor);
                instance = compiledConstructor();
                if (_recording)
                    _cachedResolutionCounts[clientType] = 1;
            }
            else {
                instance = constructor.Invoke(dependencies);
                if (_recording)
                    _uncachedResolutionCounts[clientType] = _uncachedResolutionCounts.TryGetValue(clientType, out int count) ? count + 1 : 1;
            }
            return instance;
        }

        throw new InvalidOperationException($"Could not find or resolve all dependencies (parameters) of any public constructor on requested type: {clientType.Name}.");
    }

    /// <summary>
    /// Inject all dependencies into the specified client.
    /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
    /// </summary>
    /// <param name="client">A client with service dependencies that need to be resolved.</param>
    public void ResolveDependenciesOf(object client)
    {
        throwIfInvalidState(nameof(ResolveDependenciesOf));

        // Resolve dependencies by calling every Inject method in the client's inheritance hierarchy.
        // If the client's type or any of its inherited types have cached inject methods, then
        // use/compile those as necessary so that injection is faster for future clients of these types.
        Type clientType = client.GetType();
        Type objectType = typeof(object);
        Type? cachedParentType = null;
        List<Action<object>>? compiledInjectList = null;     // Will only be initialized if this client's type or one of its parent types is cached, to save heap allocations
        do {
            // Use compiled inject methods, if they exist
            if (_compiledInject.TryGetValue(clientType, out List<Action<object>> compiledInjectMethods)) {
                for (int m = 0; m < compiledInjectMethods.Count; ++m)
                    compiledInjectMethods[m](client);
                if (_recording)
                    _cachedResolutionCounts[clientType] = _cachedResolutionCounts.TryGetValue(clientType, out int count) ? count + 1 : 1;
                return;
            }

            // Get the inject method on this type (will throw if more than one method matches)
            MethodInfo injectMethod = _typeMetadataProvider!.GetMethod(clientType, InjectMethodName, InjectMethodBindingFlags);
            if (injectMethod is null)
                goto ContinueHierarchy;

            string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? (client as U.Object)?.name ?? $"{injectMethod.DeclaringType.FullName} instance";
            object[] dependencies = getDependeciesOfMethod(clientName, injectMethod);
            if (dependencies.Length == 0)
                goto ContinueHierarchy;

            // Check if the inject method should be compiled. If so, compile/call it; otherwise, invoke it via reflection
            bool compile = true;
            if (cachedParentType is null) {
                if (_cachedResolutionTypes.Contains(clientType))
                    cachedParentType = clientType;
                else {
                    compile = false;
                    _ = injectMethod.Invoke(client, dependencies);
                    if (_recording)
                        _uncachedResolutionCounts[clientType] = _uncachedResolutionCounts.TryGetValue(clientType, out int count) ? count + 1 : 1;
                }
            }
            if (compile) {
                string compiledMethodName = $"{nameof(ResolveDependenciesOf)}_{injectMethod.DeclaringType.Name}_Generated";
                Action<object> compiledInject = _typeMetadataProvider.CompileMethodCall(compiledMethodName, nameof(client), injectMethod, dependencies);
                (compiledInjectList ??= []).Add(compiledInject);
                compiledInject(client);
                if (_recording)
                    _cachedResolutionCounts[clientType] = 1;
            }

            ContinueHierarchy:
            clientType = clientType.BaseType;
        } while (clientType is not null && clientType != objectType);

        if (cachedParentType is not null)
            _compiledInject.Add(cachedParentType, compiledInjectList!);
    }

    public void UnregisterSceneServices(Scene scene)
    {
        throwIfInvalidState(nameof(UnregisterSceneServices));

        if (!_services.TryGetValue(scene.handle, out Dictionary<Type, Dictionary<string, Service>>? sceneServices)) {
            log_UnregisterMissingSceneService(scene);
            return;
        }

        log_UnregisteringSceneServices(scene);
        int sceneServiceCount = sceneServices.Sum(x => x.Value.Values.Count);
        _ = _services.Remove(scene.handle);
        log_UnregisteredAllSceneServices(scene, sceneServiceCount);
    }

    /// <summary>
    /// Resolve the dependencies of <paramref name="method"/>.
    /// I.e., get the service that satisfies the <see cref="Type"/> and (optional) tag of each of <paramref name="method"/>'s parameters.
    /// </summary>
    /// <param name="clientName">Name of the client object instance on which <paramref name="method"/> can be called</param>
    /// <param name="method">The method for which to resolve dependencies.</param>
    /// <param name="parameters"><paramref name="method"/>'s parameters, if they have already been loaded via reflection.</param>
    /// <returns>The dependencies (parameters) required by <paramref name="method"/>.</returns>
    private object[] getDependeciesOfMethod(string clientName, MethodBase method, ParameterInfo[]? parameters = null)
    {
        _injectedTypes.Clear();
        parameters ??= _typeMetadataProvider!.GetMethodParameters(method);
        object[] dependencies = new object[parameters.Length];
        for (int p = 0; p < parameters.Length; ++p) {
            Type paramType = parameters[p].ParameterType;

            // Get the dependency's requested tag, if it exists
            InjectTagAttribute? injAttr = _typeMetadataProvider!.GetCustomAttribute<InjectTagAttribute>(parameters[p]);
            bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
            string tag = untagged ? DefaultInjectTag : injAttr!.Tag;

            // Warn if a dependency with this Type and tag has already been injected
            bool firstInjection = _injectedTypes.Add((paramType, tag));
            if (!firstInjection)
                log_MethodHasMultipleDependenciesOfType(clientName, paramType, tag);

            TryGetService(paramType, tag, clientName, out Service service);
            dependencies[p] = service.Instance;

            log_ResolvedMethodServiceParameter(clientName, tag, parameters[p]);
        }

        return dependencies;
    }
    internal void TryGetService(Type serviceType, string injectTag, string clientName, out Service service)
    {
        Dictionary<string, Service>? typedServices = null;
        foreach (int scene in _services.Keys) {
            if (_services[scene].TryGetValue(serviceType, out typedServices))
                break;
        }
        if (typedServices is null)
            throw new KeyNotFoundException($"{clientName} has a dependency of Type '{serviceType.FullName}', but no service was registered with that Type.");

        bool resolved = typedServices.TryGetValue(injectTag, out service);
        if (!resolved)
            throw new KeyNotFoundException($"{clientName} has a dependency of Type '{serviceType.FullName}' with tag '{injectTag}', but no matching service was registered. Did you forget to tag a service?");
    }

    #endregion

    public bool Disposed { get; private set; }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing) {
            // TODO: dispose services that implement IDisposable here
        }

        // Clear collections (since we can't set these readonly fields to null)
        _services.Clear();
        _injectedTypes.Clear();
        _cachedResolutionTypes.Clear();
        _compiledInject.Clear();
        _compiledConstructors.Clear();
        _uncachedResolutionCounts.Clear();
        _cachedResolutionCounts.Clear();

        Disposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Type, string, Exception?> LOG_REGISTERED_SERVICE_ACTION =
        LoggerMessage.Define<Type, string>(Information,
            new EventId(id: 0, nameof(log_RegisteredService)),
            "Successfully registered service of Type '{ServiceType}' and tag '{Tag}'"
        );
    private static readonly Action<MEL.ILogger, Type, string, string, Exception?> LOG_REGISTERED_SCENE_SERVICE_ACTION =
        LoggerMessage.Define<Type, string, string>(Information,
            new EventId(id: 0, nameof(log_RegisteredService)),
            "Successfully registered service of Type '{ServiceType}' and tag '{Tag}' from scene '{Scene}'"
        );
    private void log_RegisteredService(Service service, Scene? scene)
    {
        if (scene is null)
            LOG_REGISTERED_SERVICE_ACTION(_logger!, service.ServiceType, service.InjectTag, null);
        else
            LOG_REGISTERED_SCENE_SERVICE_ACTION(_logger!, service.ServiceType, service.InjectTag, scene.Value.name, null);
    }


    private static readonly Action<MEL.ILogger, bool, Exception?> LOG_TOGGLED_RECORDING_DEPENDENCY_RESOLUTION_ACTION =
        LoggerMessage.Define<bool>(Information,
            new EventId(id: 0, nameof(log_ToggledRecordingDependencyResolution)),
            "Recording dependency resolutions: {IsRecording}"
        );
    private void log_ToggledRecordingDependencyResolution(bool isRecording) =>
        LOG_TOGGLED_RECORDING_DEPENDENCY_RESOLUTION_ACTION(_logger!, isRecording, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_UNREGISTERING_SCENE_SERVICES_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_UnregisteringSceneServices)),
            "Unregistering services from scene '{Scene}'..."
        );
    private void log_UnregisteringSceneServices(Scene scene) => LOG_UNREGISTERING_SCENE_SERVICES_ACTION(_logger!, scene.name, null);


    private static readonly Action<MEL.ILogger, int, string, Exception?> LOG_UNREGISTERED_ALL_SCENE_SERVICES_ACTION =
        LoggerMessage.Define<int, string>(Information,
            new EventId(id: 0, nameof(log_UnregisteredAllSceneServices)),
            "Successfully unregistered all {Count} services from scene '{Scene}'"
        );
    private void log_UnregisteredAllSceneServices(Scene scene, int count) =>
        LOG_UNREGISTERED_ALL_SCENE_SERVICES_ACTION(_logger!, count, scene.name, null);


    private static readonly Action<MEL.ILogger, string, Type, string, string, Exception?> LOG_RESOLVED_METHOD_SERVICE_PARAM_ACTION =
        LoggerMessage.Define<string, Type, string, string>(Information,
            new EventId(id: 0, nameof(log_ResolvedMethodServiceParameter)),
            "Client '{Client}' had dependency of Type '{ServiceType}' and tag '{Tag}' injected into parameter '{Parameter}'"
        );
    private void log_ResolvedMethodServiceParameter(string clientName, string tag, ParameterInfo parameter) =>
        LOG_RESOLVED_METHOD_SERVICE_PARAM_ACTION(_logger!, clientName, parameter.ParameterType, tag, parameter.Name, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_UNREGISTER_MISSING_SCENE_SERVICE_ACTION =
        LoggerMessage.Define<string>(Warning,
            new EventId(id: 0, nameof(log_UnregisterMissingSceneService)),
            "Cannot unregister services from scene '{Scene}' as none have been registered. Are you trying to destroy multiple service collections from the same scene?"
        );
    private void log_UnregisterMissingSceneService(Scene scene) =>
        LOG_UNREGISTER_MISSING_SCENE_SERVICE_ACTION(_logger!, scene.name, null);


    private static readonly Action<MEL.ILogger, string, Type, string, Exception?> LOG_METHOD_HAS_MULTIPLE_DEPS_OF_TYPE_ACTION =
        LoggerMessage.Define<string, Type, string>(Warning,
            new EventId(id: 0, nameof(log_MethodHasMultipleDependenciesOfType)),
            "Client '{Client}' has multiple dependencies of Type '{ServiceType}' with tag '{Tag}'"
        );
    private void log_MethodHasMultipleDependenciesOfType(string clientName, Type serviceType, string tag) =>
        LOG_METHOD_HAS_MULTIPLE_DEPS_OF_TYPE_ACTION(_logger!, clientName, serviceType, tag, null);

    #endregion
}
