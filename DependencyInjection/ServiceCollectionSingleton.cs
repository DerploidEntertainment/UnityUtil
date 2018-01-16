using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.DependencyInjection {

    [Serializable]
    public struct Service {
        public MonoBehaviour Instance;
        [Tooltip("All services are associated with a .NET Type.  This type can be any type in the service's inheritance hierarchy.  For example, a service component derived from Monobehaviour could be associated with its actual runtime Type, with Monobehaviour, with Unity.Object, or with System.Object.  The former is most common, and is assumed if you leave this field blank.")]
        public string TypeName;
    }

    [DisallowMultipleComponent]
    public class ServiceCollectionSingleton : MonoBehaviour {

        // HIDDEN FIELDS
        private static int s_refs = 0;
        private static IDictionary<Type, MonoBehaviour> s_services = new Dictionary<Type, MonoBehaviour>();

        // INSPECTOR FIELDS
        public Service[] InitialServices;

        // EVENT HANDLERS
        private void Awake() {
            // Make sure this component is a singleton
            ++s_refs;
            Assert.IsTrue(s_refs == 1, this.GetSingletonAssertion(s_refs));

            // Add every service specified in the Inspector to the service collection
            // Each service instance will be associated with the named Type (which could be, e.g., some base class or interface type)
            // If no Type name was not provided, then use the actual name of the service's runtime instance type
            for (int s = 0; s < InitialServices.Length; ++s) {
                Service service = InitialServices[s];
                string name = service.TypeName;
                MonoBehaviour i = service.Instance;
                Type t = string.IsNullOrEmpty(name) ? i.GetType() : Type.GetType(name);
                s_services.Add(t, i);
            }
        }

        // INTERFACE
        public void AddService<T>(T service) where T : MonoBehaviour => s_services.Add(typeof(T), service);

        public void ResolveDependencies(MonoBehaviour component) {
            // Get every IDependsOn<> generic interface on this component
            // Call each one's Inject() method, passing in the associated service
            MethodInfo injectGeneric = null;
            Type[] interfaces = component.GetType().GetInterfaces();
            for (int i=0; i < interfaces.Length; ++i) {
                Type dependsOn = interfaces[i];
                if (dependsOn.IsGenericType) {
                    Type dependsOnGeneric = dependsOn.GetGenericTypeDefinition();
                    if (dependsOnGeneric.Name == nameof(IDependsOn<MonoBehaviour>) + "`1") {    // This is the name of a 1-parameter generic interface (the generic type parameter in the nameof expression doesn't actually matter)
                        MethodInfo inject = dependsOnGeneric.GetMethod(nameof(IDependsOn<MonoBehaviour>.Inject));      // Again, the generic type parameter in the nameof expression doesn't actually matter
                        Type depType = inject.GetGenericArguments()[0];
                        bool serviceRegistered = s_services.TryGetValue(depType, out MonoBehaviour dependency);
                        if (serviceRegistered) {
                            injectGeneric = inject.MakeGenericMethod(depType);
                            injectGeneric.Invoke(component, new object[] { dependency });
                        }
                    }
                }
            }
        }
        public void ResolveDependencies(GameObject gameObject, bool resolveChildren) {
            MonoBehaviour[] components = resolveChildren ? gameObject.GetComponentsInChildren<MonoBehaviour>() : gameObject.GetComponents<MonoBehaviour>();
            for (int c=0; c<components.Length; ++c)
                ResolveDependencies(components[c]);
        }
        public void ResolveDependencies<T>(IDependsOn<T> client) where T : MonoBehaviour {
            bool serviceRegistered = s_services.TryGetValue(typeof(T), out MonoBehaviour dependency);
            if (serviceRegistered)
                client.Inject(dependency as T);
        }

    }

}
