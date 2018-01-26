﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityUtil {

    [Serializable]
    public struct Service {
        [Tooltip("Optional.  All services are associated with a System.Type.  This Type can be any Type in the service's inheritance hierarchy, but must be a [Serializable] Type.  For example, a service component derived from Monobehaviour could be associated with its actual declared Type, with Monobehaviour, or with UnityEngine.Object.  The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        public MonoBehaviour Instance;
    }

    public class DependencyInjector : MonoBehaviour {

        // HIDDEN FIELDS
        private IDictionary<Type, IDictionary<string, MonoBehaviour>> _services = new Dictionary<Type, IDictionary<string, MonoBehaviour>>();

        // INSPECTOR FIELDS
        [Tooltip("Determines how this " + nameof(UnityUtil.DependencyInjector) + " locates clients.  Changing this to a value other than " + nameof(DependencyInjectionMode.EntireScene) + " and supplying client objects in the " + nameof(DependencyInjector.Clients) + " array can significantly speed up injection times.")]
        public DependencyInjectionMode InjectionMode = DependencyInjectionMode.EntireScene;
        [Tooltip("The service collection from which dependencies will be resolved")]
        public Service[] Services;
        [Tooltip("The specific GameObjects that require (and/or whose children require) dependency injection.  Only required if " + nameof(DependencyInjector.InjectionMode) + " has a value other than " + nameof(DependencyInjectionMode.EntireScene) + ".")]
        public GameObject[] Clients;

        // INTERFACE
        public void Inject() {
            // Add every service specified in the Inspector to the private service collection
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was provided, then use the actual name of the service's runtime instance type
            ConditionalLogger.Log($"Refreshing configured services...", false);
            int successes = 0;
            _services.Clear();
            for (int s = 0; s < Services.Length; ++s) {
                Service service = Services[s];
                MonoBehaviour instance = service.Instance;

                // Get the service's Type
                Type type;
                if (string.IsNullOrEmpty(service.TypeName))
                    type = service.Instance.GetType();
                else {
                    try {
                        type = Type.GetType(service.TypeName);
                        if (type == null)
                            throw new InvalidOperationException($"Could not load Type '{service.TypeName}'.  Make sure you provided its fully qualified name.");
                        if (!type.IsAssignableFrom(service.Instance.GetType()))
                            throw new InvalidOperationException($"The service instance configured for Type '{service.TypeName}' is not actually derived from that Type!");
                        if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
                            if (type.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length == 0)
                                throw new InvalidOperationException($"Type '{service.TypeName}' is not Serializable nor derived from UnityEngine.Object.");
                    }
                    catch(Exception ex) {
                        ConditionalLogger.LogError($"Could not configure service of Type '{service.TypeName}': {ex.Message}");
                        continue;
                    }
                }

                // Add the service to the service collection, throwing an error if it's Type/Tag have already been configured
                bool typeAdded = _services.TryGetValue(type, out IDictionary<string, MonoBehaviour> typedServices);
                if (typeAdded) {
                    bool tagAdded = typedServices.TryGetValue(instance.tag, out MonoBehaviour taggedService);
                    if (tagAdded) {
                        ConditionalLogger.LogError($"Multiple services were configured with Type '{type.Name}' and tag '{instance.tag}'", framePrefix: false);
                        continue;
                    }
                    else {
                        typedServices.Add(instance.tag, instance);
                        ++successes;
                    }
                }
                else {
                    _services.Add(type, new Dictionary<string, MonoBehaviour> { { instance.tag, instance } });
                    ++successes;
                }
            }
            string successMsg = $" successfully configured {successes} out of {Services.Length} services.";
            if (successes == Services.Length)
                this.Log(successMsg, framePrefix: false);
            else {
                string errorMsg = $"Please resolve the above errors and try again.";
                this.Log($"{successMsg}  {errorMsg}", framePrefix: false);
                return;
            }

            // Determine which components require dependecies, based on the injection mode
            ConditionalLogger.Log($"Searching for client components...", false);
            IEnumerable<MonoBehaviour> clients;
            switch (InjectionMode) {
                case DependencyInjectionMode.SpecifiedClients:
                    clients = Clients.SelectMany(c => c.GetComponents<MonoBehaviour>());
                    break;

                case DependencyInjectionMode.SpecifiedClientsPlusChildren:
                    clients = Clients.SelectMany(c => c.GetComponentsInChildren<MonoBehaviour>());
                    break;

                case DependencyInjectionMode.EntireScene:
                    clients = SceneManager.GetActiveScene()
                                          .GetRootGameObjects()
                                          .SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>());
                    break;

                default:
                    throw new NotImplementedException(ConditionalLogger.GetSwitchDefault(InjectionMode));
            }
            ConditionalLogger.Log($"Found {clients.Count()} potential client components", false);

            // For each client component, get the actual dependency fields/properties,
            // then resolve and inject those dependencies!
            int attempts = 0;
            successes = 0;
            BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance;
            foreach (MonoBehaviour client in clients) {
                FieldInfo[] fields = client.GetType().GetFields(bindings);
                var injectedTypes = new HashSet<Type>();
                foreach (FieldInfo field in fields) {
                    // If this isn't field doesn't have a service dependency then skip it
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
                    ++attempts;
                    bool resolved = _services.TryGetValue(field.FieldType, out IDictionary<string, MonoBehaviour> typedServices);
                    if (!resolved) {
                        field.SetValue(client, null);
                        ConditionalLogger.LogError($"No services configured with Type '{field.FieldType.Name}'", framePrefix: false);
                        continue;
                    }
                    var injAttr = attrs[0] as InjectAttribute;
                    string tag = string.IsNullOrEmpty(injAttr.Tag) ? "Untagged" : injAttr.Tag;
                    resolved = typedServices.TryGetValue(tag, out MonoBehaviour service);
                    if (!resolved) {
                        field.SetValue(client, null);
                        ConditionalLogger.LogError($"No services configured with Type '{field.FieldType.Name}' and tag '{tag}'", framePrefix: false);
                        continue;
                    }

                    // If this dependency has not already been correctly injected, then inject it now with a log message
                    field.SetValue(client, service);
                    ++successes;
                    client.Log($" injected dependency of Type '{field.FieldType.Name}' with tag '{tag}' into field '{field.Name}'.", framePrefix: false);
                }
            }

            // Log how many new dependency injections were made
            this.Log($" successfully resolved {successes} out of {attempts} dependencies", framePrefix: false);
        }

    }

}
