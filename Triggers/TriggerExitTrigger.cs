using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Triggers {

    [RequireComponent(typeof(Collider))]
    public class TriggerExitTrigger : MonoBehaviour {

        public Collider TriggerCollider { get; private set; }

        public TriggerColliderEvent ColliderExited = new TriggerColliderEvent();

        private void Awake() {
            TriggerCollider = GetComponent<Collider>();
            Assert.IsTrue(TriggerCollider.isTrigger, $"{this.GetHierarchyNameWithType()} is associated with a Collider, but the Collider is not a trigger!");
        }

        private void OnTriggerExit(Collider other) => ColliderExited.Invoke(other);

    }

}
