using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ComponentLifecycleTrigger : MonoBehaviour
    {
        public UnityEvent Awoken = new();
        public UnityEvent Started = new();
        public UnityEvent Enabled = new();
        public UnityEvent Disabled = new();
        public UnityEvent Destroyed = new();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() => Awoken.Invoke();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Start() => Started.Invoke();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnEnable() => Enabled.Invoke();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnDisable() => Disabled.Invoke();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnDestroy() => Destroyed.Invoke();

    }

}
