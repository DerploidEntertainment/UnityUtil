using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers {

    public class ComponentLifecycleTrigger : MonoBehaviour {

        // INSPECTOR FIELDS
        public UnityEvent Awoken = new UnityEvent();
        public UnityEvent Started = new UnityEvent();
        public UnityEvent Enabled = new UnityEvent();
        public UnityEvent Disabled = new UnityEvent();
        public UnityEvent Destroyed = new UnityEvent();

        // EVENT HANDLERS
        private void Awake() => Awoken.Invoke();
        private void Start() => Started.Invoke();
        private void OnEnable() => Enabled.Invoke();
        private void OnDisable() => Disabled.Invoke();
        private void OnDestroy() => Destroyed.Invoke();

    }

}
