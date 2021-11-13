using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ComponentLifecycleTrigger : MonoBehaviour {

        // INSPECTOR FIELDS
        public UnityEvent Awoken = new UnityEvent();
        public UnityEvent Started = new UnityEvent();
        public UnityEvent Enabled = new UnityEvent();
        public UnityEvent Disabled = new UnityEvent();
        public UnityEvent Destroyed = new UnityEvent();

        // EVENT HANDLERS
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
