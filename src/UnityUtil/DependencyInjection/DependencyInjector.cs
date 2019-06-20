using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Assertions;
using U = UnityEngine;

namespace UnityEngine {

    [Serializable]
    public struct Service {
        [Tooltip("Optional.  All services are associated with a System.Type.  This Type can be any Type in the service's inheritance hierarchy, but must be a [Serializable] Type.  For example, a service component derived from Monobehaviour could be associated with its actual declared Type, with Monobehaviour, or with UnityEngine.Object.  The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        [HideInInspector, NonSerialized]
        public string Tag;
        public MonoBehaviour Instance;
    }

    public class DependencyInjector : MonoBehaviour {

        // HIDDEN FIELDS
        private static readonly IDictionary<Type, IDictionary<string, Service>> s_services = new Dictionary<Type, IDictionary<string, Service>>();

        // INSPECTOR FIELDS
        [Tooltip("The service collection from which dependencies will be resolved")]
        public Service[] ServiceCollection;

        // INTERFACE
        public const string RequiredTag = "DependencyInjector";
        public const string InjectMethodName = "Inject";
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(params MonoBehaviour[] clients) => ResolveDependenciesOf(clients as IEnumerable<MonoBehaviour>);
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public static void ResolveDependenciesOf(IEnumerable<MonoBehaviour> clients) {
            foreach (MonoBehaviour client in clients) {
                MethodInfo[] injectMethods = client.GetType().GetMethods().Where(m => m.Name == InjectMethodName).ToArray();
                for (int m = 0; m < injectMethods.Length; ++m)
                    invokeInject(injectMethods[m], client);
            }
        }

        // EVENT HANDLERS
        private void Awake() {
            // Make sure we are correctly tagged
            Assert.AreEqual(RequiredTag, tag, $"{this.GetHierarchyNameWithType()} must be tagged '{DependencyInjector.RequiredTag}', not '{tag}'!");

            // Add every service specified in the Inspector to the private service collection
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            this.Log($" awaking, adding services...");
            for (int s = 0; s < ServiceCollection.Length; ++s) {
                Service service = ServiceCollection[s];

                // Update the service's Type/Tag
                if (string.IsNullOrEmpty(service.TypeName))
                    service.TypeName = service.Instance.GetType().AssemblyQualifiedName;
                service.Tag = service.Instance.tag;
                ServiceCollection[s] = service;

                // Get the service's Type, if it is valid
                Type type;
                try {
                    type = Type.GetType(service.TypeName);
                    if (type == null)
                        throw new InvalidOperationException($"Could not load Type '{service.TypeName}'.  Make sure that you provided its fully qualified name and that its assembly is laoded.");
                    if (!type.IsAssignableFrom(service.Instance.GetType()))
                        throw new InvalidOperationException($"The service instance configured for Type '{service.TypeName}' is not actually derived from that Type!");
                    if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
                        if (type.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length == 0)
                            throw new InvalidOperationException($"Type '{service.TypeName}' is not Serializable nor derived from UnityEngine.Object.");
                }
                catch (Exception ex) {
                    this.LogError($" could not configure service of Type '{service.TypeName}': {ex.Message}");
                    continue;
                }

                // Add the service to the service collection, throwing an error if it's Type/Tag have already been configured
                bool typeAdded = s_services.TryGetValue(type, out IDictionary<string, Service> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(service.Tag, out _);
                    if (tagAdded) {
                        this.LogError($" configured multiple services with Type '{service.TypeName}' and tag '{service.Tag}'");
                        continue;
                    }
                    else {
                        typedServices.Add(service.Tag, service);
                        this.Log($" successfully configured service of type '{service.TypeName}' and tag '{service.Tag}'.", framePrefix: false);
                    }
                }
                else
                    s_services.Add(type, new Dictionary<string, Service> { { service.Tag, service } });
            }
        }
        private void OnDestroy() {
            // Remove every service specified in the Inspector from the private service collection
            this.Log($" being destroyed, removing services...");
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
                        this.LogError($" could not remove service of Type '{service.TypeName}': {ex.Message}");
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
                        this.LogError($" couldn't remove service with Type '{type}' and tag '{service.Tag}' because somehow it wasn't present in the service collection!");
                        continue;
                    }
                }
                else {
                    this.LogError($" couldn't remove service of Type '{type}' because somehow it wasn't present in the service collection!");
                    continue;
                }
            }

            // Log whether or not all services were removed successfully
            string successMsg = $" successfully removed {successes} out of {ServiceCollection.Length} services.";
            if (successes == ServiceCollection.Length)
                this.Log(successMsg);
            else
                this.LogError(successMsg);
        }

        private static void invokeInject(MethodInfo injectMethod, MonoBehaviour client) {
            var injectedTypes = new HashSet<Type>();

            ParameterInfo[] @params = injectMethod.GetParameters();
            var services = new U.Object[@params.Length];
            for (int p = 0; p < @params.Length; ++p) {
                ParameterInfo param = @params[p];
                Type pType = param.ParameterType;

                // If this parameter's Type is not Serializable nor derived from UnityEngine.Object, then skip it with an error
                if (!pType.IsSubclassOf(typeof(U.Object))) {
                    if (pType.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length == 0) {
                        client.LogError($" has a dependency of type '{pType.FullName}', which is neither Serializable, nor derived from UnityEngine.Object.", framePrefix: false);
                        continue;
                    }
                }

                // Warn if a dependency with this Type has already been injected
                bool firstInjection = injectedTypes.Add(pType);
                if (!firstInjection)
                    client.LogWarning($" has multiple dependencies of Type '{pType.FullName}'.", framePrefix: false);

                // If this dependency can't be resolved, then skip it with an error and clear the field
                bool resolved = s_services.TryGetValue(pType, out IDictionary<string, Service> typedServices);
                if (!resolved) {
                    client.LogError($" has a dependency of Type '{pType.FullName}', but no service was registered with that Type.  Did you forget to add a service to the service collection, or put " + nameof(UnityEngine.DependencyInjector) + " first in the project's Script Execution Order?", framePrefix: false);
                    continue;
                }
                InjectTagAttribute injAttr = param.GetCustomAttribute<InjectTagAttribute>();
                bool untagged = string.IsNullOrEmpty(injAttr?.Tag);
                string tag = untagged ? "Untagged" : injAttr.Tag;
                resolved = typedServices.TryGetValue(tag, out Service service);
                if (!resolved) {
                    client.LogError($" has a dependency of Type '{pType.FullName}' with tag '{tag}', but no matching service was registered. Did you incorrectly tag a service, or forget to put " + nameof(UnityEngine.DependencyInjector) + " first in the project's Script Execution Order?", framePrefix: false);
                    continue;
                }

                // Log that this dependency has been resolved
                services[p] = service.Instance;
                client.Log($" had dependency of Type '{pType.FullName}'{(untagged ? "" : " with tag '{tag}'")} injected into field '{param.Name}'.", framePrefix: false);
            }

            injectMethod.Invoke(client, services);
        }

    }

}
