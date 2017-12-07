using UnityEngine;

namespace Danware.Unity.DependencyInjection {

    public class DependencyInjector : MonoBehaviour {

        public ServiceCollectionSingleton ServiceCollection;
        [Tooltip("If true, then all " + nameof(IDependsOn<MonoBehaviour>) + " components on this GameObject and all of its children will have their dependencies injected.  If false, then only this GameObject will have its dependencies injected.")]
        public bool InjectChildDependencies = true;

        private void Awake() {
            MonoBehaviour[] components = InjectChildDependencies ? GetComponentsInChildren<MonoBehaviour>() : GetComponents<MonoBehaviour>();
            for (int c = 0; c < components.Length; ++c)
                ServiceCollection.ResolveDependencies(gameObject, InjectChildDependencies);
        }

    }

}
