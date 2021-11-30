using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ColliderEnterTrigger : ColliderTriggerBase
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnCollisionEnter(Collision other) => TryTrigger(other.rigidbody);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnTriggerEnter(Collider other) => TryTrigger(other.attachedRigidbody);
    }

    public class ColliderExitTrigger : ColliderTriggerBase
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnCollisionExit(Collision other) => TryTrigger(other.rigidbody);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnTriggerExit(Collider other) => TryTrigger(other.attachedRigidbody);
    }

    [RequireComponent(typeof(Collider))]
    public abstract class ColliderTriggerBase : MonoBehaviour {

        public UnityEvent Triggered = new();

        public Collider AttachedCollider { get; private set; }

        [Tooltip(
            "If non-null, this value will be used to filter collider events to only those where the attached Rigidbody of " +
            $"the interacting Collider MATCHES or DOES NOT MATCH this Tag, depending on the value of {nameof(FilterIsBlacklist)}."
        )]
        public string AttachedRigidbodyTagFilter;
        [Tooltip(
            $"If true, then the {nameof(AttachedRigidbodyTagFilter)} will be used as a blacklist " +
            "(i.e., any interacting Collider will raise the UnityEvent EXCEPT those with an attached Rigidbody matching that Tag); " +
            "if false, then {nameof(AttachedRigidbodyTagFilter)} will be used as whitelist " +
            "(i.e., only interacting Colliders with attached Rigidbodies MATCHING the filter will raise the event.)"
        )]
        public bool FilterIsBlacklist = false;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() => AttachedCollider = GetComponent<Collider>();

        protected void TryTrigger(Rigidbody rb) {
            bool matches =
                rb is null
                || string.IsNullOrEmpty(AttachedRigidbodyTagFilter)
                || (FilterIsBlacklist && !rb.CompareTag(AttachedRigidbodyTagFilter))
                || (!FilterIsBlacklist && rb.CompareTag(AttachedRigidbodyTagFilter));
            if (matches)
                Triggered.Invoke();
        }

    }

}
