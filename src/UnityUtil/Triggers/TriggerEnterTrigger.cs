using System;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    [Serializable]
    public class TriggerColliderEvent : UnityEvent<Collider> { }

    [RequireComponent(typeof(Collider))]
    public class TriggerEnterTrigger : MonoBehaviour {

        public Collider TriggerCollider { get; private set; }

        [Tooltip("If non-null, this value will be used to filter OnTriggerEnter events to only those where the attached Rigidbody of the enterring Collider MATCHES or DOES NOT MATCH this Tag, depending on the value of " + nameof(TriggerEnterTrigger.FilterIsBlacklist) + ".")]
        public string AttachedRigidbodyTagFilter;
        [Tooltip("If true, then the " + nameof(TriggerEnterTrigger.AttachedRigidbodyTagFilter) + " will be used as a blacklist (i.e., any enterring Collider will raise the UnityEvent EXCEPT those with an attached Rigidbody matching that Tag); if false, then " + nameof(TriggerEnterTrigger.AttachedRigidbodyTagFilter) + " will be used as whitelist (i.e., only enterring Colliders with attached Rigidbodies MATCHING the filter will raise the event.)")]
        public bool FilterIsBlacklist = false;
        public TriggerColliderEvent ColliderEnterred = new TriggerColliderEvent();

        private void Awake() {
            TriggerCollider = GetComponent<Collider>();
            Assert.IsTrue(TriggerCollider.isTrigger, $"{this.GetHierarchyNameWithType()} is associated with a Collider, but the Collider is not a trigger!");
        }
        private void OnTriggerEnter(Collider other) {
            Rigidbody rb = other.attachedRigidbody;
            bool raiseEvent =
                rb == null ||
                string.IsNullOrEmpty(AttachedRigidbodyTagFilter) ||
                (FilterIsBlacklist && !rb.CompareTag(AttachedRigidbodyTagFilter)) ||
                (!FilterIsBlacklist && rb.CompareTag(AttachedRigidbodyTagFilter));
            if (raiseEvent)
                ColliderEnterred.Invoke(other);
        }

    }

}
