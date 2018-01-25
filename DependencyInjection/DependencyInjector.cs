using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityUtil {

    /// <summary>
    /// Determines how this <see cref="DependencyInjector"/> locates clients
    /// </summary>
    public enum DependencyInjectionMode {
        /// <summary>
        /// Required dependencies will only be injected into the <see cref="GameObject"/>s specified in the <see cref="DependencyInjector.Clients"/> array.  Use this if you know, at design time, exactly which <see cref="GameObject"/>s require dependencies.
        /// </summary>
        SpecifiedClients,

        /// <summary>
        /// Required dependencies will be injected into the <see cref="GameObject"/>s specified in the <see cref="DependencyInjector.Clients"/> array, as well as all of their child objects (recursively).  Use this if you know that all objects requiring dependencies will be parented to a specific <see cref="GameObject"/>.
        /// </summary>
        SpecifiedClientsPlusChildren,

        /// <summary>
        /// Every <see cref="GameObject"/> in the Scene will be checked for required dependencies, and have those dependencies injected.  This is the easiest option, guaranteeing that all <see cref="MonoBehaviour"/>s in the scene will be associated with their correct dependencies, but is also the slowest slowest option, but 
        /// </summary>
        EntireScene
    }

    [Serializable]
    public struct Service {
        [Tooltip("Optional.  All services are associated with a System.Type.  This Type can be any Type in the service's inheritance hierarchy.  For example, a service component derived from Monobehaviour could be associated with its actual runtime Type, with Monobehaviour, with Unity.Object, or with System.Object.  The actual declared Type is assumed if you leave this field blank.")]
        public string TypeName;
        public MonoBehaviour Instance;
    }

    /// <summary>
    /// Inject the service configured with this field's <see cref="Type"/> and an optional Inspector tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class InjectAttribute : PropertyAttribute {

        /// <summary></summary>
        public InjectAttribute() { }
        /// <summary></summary>
        /// <param name="tag">The service <see cref="MonoBehaviour"/> with this tag (set in the Inspector) will be injected.  Use when there must be multiple services with the same Type.</param>
        public InjectAttribute(string tag) {
            Tag = tag;
        }

        /// <summary>
        /// The service <see cref="MonoBehaviour"/> with this tag (set in the Inspector) will be injected.  Use when there must be multiple services with the same Type.
        /// </summary>
        public string Tag { get; }
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
                Type type = string.IsNullOrEmpty(service.TypeName) ? service.Instance.GetType() : Type.GetType(service.TypeName);
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

                    // If this field isn't of a Type derived from MonoBehaviour then skip it with an error
                    if (!field.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                        client.LogError($" tried to inject dependency into field \"{field.Name}\", which does not have a Type derived from MonoBehaviour.", framePrefix: false);
                        continue;
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
