using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Logging;

namespace UnityEngine {

    [Serializable]
    public struct Service {
        #pragma warning disable CA2235 // Mark all non-serializable fields

        [Tooltip("Optional. All services are associated with a System.Type.  This Type can be any Type in the service's inheritance hierarchy.  For example, a service component derived from Monobehaviour could be associated with its actual declared Type, with Monobehaviour, or with UnityEngine.Object. The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        [HideInInspector, NonSerialized]
        public string Tag;
        public MonoBehaviour Instance;

        #pragma warning restore CA2235 // Mark all non-serializable fields
    }

    public class DependencyInjector : MonoBehaviour {

        private static readonly IDictionary<Type, IDictionary<string, Service>> s_services = new Dictionary<Type, IDictionary<string, Service>>();
        private static DependencyInjector s_instance;

        [Tooltip("If true, then this component will log its own progress using Debug.Log. If false, this component will not log anything.")]
        public bool Logging = true;
        [Tooltip("The service collection from which dependencies will be resolved")]
        public Service[] ServiceCollection;

        public const string InjectMethodName = "Inject";

        /// <summary>
        /// Inject all dependencies into the specified client.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="client">A client with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(Object client) => ResolveDependenciesOf(new[] { client });
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(IEnumerable<Object> clients) {
            if (s_instance == null) {
                s_instance = FindObjectOfType<DependencyInjector>();
                buildServiceCollection(s_instance.ServiceCollection);
            }

            foreach (Object client in clients) {
                MethodInfo[] injectMethods = client.GetType().GetMethods().Where(m => m.Name == InjectMethodName).ToArray();
                for (int m = 0; m < injectMethods.Length; ++m)
                    s_instance.invokeInject(injectMethods[m], client);
            }
        }

        // EVENT HANDLERS
        private static void buildServiceCollection(IList<Service> services) {
            // Add every service specified in the Inspector to the private service collection
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            log(LogType.Log, $"Awaking, adding services...");
            for (int s = 0; s < services.Count; ++s) {
                Service service = services[s];

                // Update the service's Type/Tag
                if (string.IsNullOrEmpty(service.TypeName))
                    service.TypeName = service.Instance.GetType().AssemblyQualifiedName;
                service.Tag = service.Instance.tag;
                services[s] = service;

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
        private void OnDestroy() {
            // Remove every service specified in the Inspector from the private service collection
            log(LogType.Log, $"Being destroyed, removing services...");
            int successes = 0;
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
                        ++successes;
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

            // Log whether or not all services were removed successfully
            if (successes == ServiceCollection.Length)
                log(LogType.Log, $"Successfully removed all {ServiceCollection.Length} services.");
            else
                log(LogType.Error, $"Removed {successes} out of {ServiceCollection.Length} services.");
        }

        private static void log(LogType logType, object message) {
            if (s_instance.Logging)
                Debug.unityLogger.Log(logType, message);
        }
        private void invokeInject(MethodInfo injectMethod, Object client) {
            var injectedTypes = new HashSet<Type>();

            ParameterInfo[] @params = injectMethod.GetParameters();
            var services = new Object[@params.Length];
            for (int p = 0; p < @params.Length; ++p) {
                ParameterInfo param = @params[p];
                Type pType = param.ParameterType;
                string clientName = (client as MonoBehaviour)?.GetHierarchyNameWithType() ?? client.name;

                // Warn if a dependency with this Type has already been injected
                bool firstInjection = injectedTypes.Add(pType);
                if (!firstInjection)
                    log(LogType.Warning, $"{clientName} has multiple dependencies of Type '{pType.FullName}'.");

                // If this dependency can't be resolved, then skip it with an error and clear the field
                bool resolved = s_services.TryGetValue(pType, out IDictionary<string, Service> typedServices);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{pType.FullName}', but no service was registered with that Type. Did you forget to add a service to the service collection, or put " + nameof(UnityEngine.DependencyInjector) + " first in the project's Script Execution Order?");
                    continue;
                }
                InjectTagAttribute injAttr = param.GetCustomAttribute<InjectTagAttribute>();
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? "Untagged" : injAttr.Tag;
                resolved = typedServices.TryGetValue(tag, out Service service);
                if (!resolved) {
                    log(LogType.Error, $"{clientName} has a dependency of Type '{pType.FullName}' with tag '{tag}', but no matching service was registered. Did you incorrectly tag a service, or forget to put " + nameof(UnityEngine.DependencyInjector) + " first in the project's Script Execution Order?");
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
