using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    [Serializable]
    public struct Service {
        [Tooltip("Optional.  All services are associated with a System.Type.  This Type can be any Type in the service's inheritance hierarchy, but must be a [Serializable] Type.  For example, a service component derived from Monobehaviour could be associated with its actual declared Type, with Monobehaviour, or with UnityEngine.Object.  The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        public MonoBehaviour Instance;
    }

    public class DependencyInjector : MonoBehaviour {

        // HIDDEN FIELDS
        private static IDictionary<Type, IDictionary<string, MonoBehaviour>> s_services = new Dictionary<Type, IDictionary<string, MonoBehaviour>>();

        // INSPECTOR FIELDS
        [Tooltip("The service collection from which dependencies will be resolved")]
        public Service[] ServiceCollection;

        // INTERFACE
        public static string Tag = "DependencyInjector";
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public void Inject(params MonoBehaviour[] clients) => Inject(clients as IEnumerable<MonoBehaviour>);
        /// <summary>
        /// Inject all dependencies into the specified clients.
        /// Can be called at runtime to satisfy dependencies of procedurally generated components, e.g., by a spawner.
        /// </summary>
        /// <param name="clients">A collection of clients with service dependencies that need to be resolved.</param>
        public void Inject(IEnumerable<MonoBehaviour> clients) {
            var injectedTypes = new HashSet<Type>();
            BindingFlags fieldBindingFlags = BindingFlags.Instance | BindingFlags.Public;

            // For each client component, get the actual dependency fields/properties,
            // then resolve and inject those dependencies!
            foreach (MonoBehaviour client in clients) {
                injectedTypes.Clear();
                FieldInfo[] fields = client.GetType().GetFields(fieldBindingFlags);
                foreach (FieldInfo field in fields) {
                    // If this field doesn't have a service dependency then skip it
                    object[] attrs = field.GetCustomAttributes(typeof(InjectAttribute), inherit: false);
                    if (attrs.Length == 0)
                        continue;

                    // If this field's Type is not Serializable nor derived from UnityEngine.Object, then skip it with an error
                    if (!field.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) {
                        if (field.FieldType.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length == 0) {
                            client.LogError($" tried to inject dependency into field '{field.Name}', whose Type is not Serializable, nor derived from UnityEngine.Object.", framePrefix: false);
                            continue;
                        }
                    }

                    // Warn if a dependency with this Type has already been injected
                    bool firstInjection = injectedTypes.Add(field.FieldType);
                    if (!firstInjection)
                        client.LogWarning($" injecting multiple dependencies of Type '{field.FieldType.Name}'.", framePrefix: false);

                    // If this dependency can't be resolved, then skip it with an error and clear the field
                    bool resolved = s_services.TryGetValue(field.FieldType, out IDictionary<string, MonoBehaviour> typedServices);
                    if (!resolved) {
                        field.SetValue(client, null);
                        BetterLogger.LogError($"No services configured with Type '{field.FieldType.Name}'.  Did you incorrectly tag a service or forget to put " + nameof(UnityUtil.DependencyInjector) + " first in the project's Script Execution Order?", framePrefix: false);
                        continue;
                    }
                    var injAttr = attrs[0] as InjectAttribute;
                    bool untagged = string.IsNullOrEmpty(injAttr.Tag);
                    string tag = untagged ? "Untagged" : injAttr.Tag;
                    resolved = typedServices.TryGetValue(tag, out MonoBehaviour service);
                    if (!resolved) {
                        field.SetValue(client, null);
                        BetterLogger.LogError($"No services configured with Type '{field.FieldType.Name}' and tag '{tag}'", framePrefix: false);
                        continue;
                    }

                    // If this dependency has not already been correctly injected, then inject it now with a log message
                    field.SetValue(client, service);
                    client.Log($" injected dependency of Type '{field.FieldType.Name}'{(untagged ? "" : " with tag '{tag}'")} into field '{field.Name}'.", framePrefix: false);
                }
            }
        }

        // EVENT HANDLERS
        private void Awake() {
            // Make sure we are correctly tagged
            Assert.AreEqual(Tag, tag, $"{this.GetHierarchyNameWithType()} must be tagged '{DependencyInjector.Tag}', not '{tag}'!");

            // Add every service specified in the Inspector to the private service collection
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            this.Log($" awaking, adding services...");
            int successes = 0;
            foreach (Service service in ServiceCollection) {
                MonoBehaviour instance = service.Instance;

                // Get the service's Type, if it is valid
                Type type;
                if (string.IsNullOrEmpty(service.TypeName))
                    type = service.Instance.GetType();
                else {
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
                }

                // Add the service to the service collection, throwing an error if it's Type/Tag have already been configured
                bool typeAdded = s_services.TryGetValue(type, out IDictionary<string, MonoBehaviour> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(instance.tag, out MonoBehaviour taggedService);
                    if (tagAdded) {
                        this.LogError($" configured multiple services with Type '{type.Name}' and tag '{instance.tag}'");
                        continue;
                    }
                    else {
                        typedServices.Add(instance.tag, instance);
                        ++successes;
                    }
                }
                else {
                    s_services.Add(type, new Dictionary<string, MonoBehaviour> { { instance.tag, instance } });
                    ++successes;
                }
            }

            // Log whether or not all services were configured successfully
            string successMsg = $" successfully configured {successes} out of {ServiceCollection.Length} services.";
            if (successes == ServiceCollection.Length)
                this.Log(successMsg);
            else {
                string errorMsg = $"Please resolve the above errors and try again.";
                this.LogError($"{successMsg}  {errorMsg}");
            }
        }
        private void OnDestroy() {
            // Remove every service specified in the Inspector from the private service collection
            this.Log($" being destroyed, removing services...");
            int successes = 0;
            foreach (Service service in ServiceCollection) {
                MonoBehaviour instance = service.Instance;

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
                bool typeAdded = s_services.TryGetValue(type, out IDictionary<string, MonoBehaviour> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(instance.tag, out MonoBehaviour taggedService);
                    if (tagAdded) {
                        typedServices.Remove(instance.tag);
                        if (typedServices.Count == 0)
                            s_services.Remove(type);
                        ++successes;
                    }
                    else {
                        this.LogError($" couldn't remove service with Type '{type}' and tag '{instance.tag}' because somehow it wasn't present in the service collection!");
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

    }

}
