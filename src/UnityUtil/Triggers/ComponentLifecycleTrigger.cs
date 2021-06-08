using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ComponentLifecycleTrigger : MonoBehaviour {

        // INSPECTOR FIELDS
        public UnityEvent Awoken = new();
        public UnityEvent Started = new();
        public UnityEvent Enabled = new();
        public UnityEvent Disabled = new();
        public UnityEvent Destroyed = new();

        // EVENT HANDLERS
        private void Awake() => Awoken.Invoke();
        private void Start() => Started.Invoke();
        private void OnEnable() => Enabled.Invoke();
        private void OnDisable() => Disabled.Invoke();
        private void OnDestroy() => Destroyed.Invoke();

    }

}
