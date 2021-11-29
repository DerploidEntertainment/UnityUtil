using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ColliderEnterTrigger2D : ColliderTriggerBase2D {
        private void OnCollisionEnter2D(Collision2D other) => TryTrigger(other.rigidbody);
        private void OnTriggerEnter2D(Collider2D other) => TryTrigger(other.attachedRigidbody);
    }

    public class ColliderExitTrigger2D : ColliderTriggerBase2D {
        private void OnCollisionExit2D(Collision2D other) => TryTrigger(other.rigidbody);
        private void OnTriggerExit2D(Collider2D other) => TryTrigger(other.attachedRigidbody);
    }

    [RequireComponent(typeof(Collider2D))]
    public abstract class ColliderTriggerBase2D : MonoBehaviour {

        public UnityEvent Triggered = new();

        public Collider2D AttachedCollider { get; private set; }

        [Tooltip(
            "If non-null, this value will be used to filter collider events to only those where the attached Rigidbody of the interacting Collider " +
            $"MATCHES or DOES NOT MATCH this Tag, depending on the value of {nameof(FilterIsBlacklist)}."
        )]
        public string AttachedRigidbodyTagFilter;

        [Tooltip(
            $"If true, then the {nameof(AttachedRigidbodyTagFilter)} will be used as a blacklist " +
            "(i.e., any interacting Collider will raise the UnityEvent EXCEPT those with an attached Rigidbody matching that Tag); " +
            $"if false, then {nameof(AttachedRigidbodyTagFilter)} will be used as whitelist " +
            "(i.e., only interacting Colliders with attached Rigidbodies MATCHING the filter will raise the event.)"
        )]
        public bool FilterIsBlacklist = false;

        private void Awake() => AttachedCollider = GetComponent<Collider2D>();

        protected void TryTrigger(Rigidbody2D rb) {
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
