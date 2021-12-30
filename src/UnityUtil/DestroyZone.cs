using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace UnityEngine {

    [RequireComponent(typeof(Collider))]
    public class DestroyZone : MonoBehaviour {

        [Tooltip(
            "If true, then the GameObject of the Rigidbody attached to the triggering Collider will be destroyed. " +
            "If false, then only the GameObject of the trigger Collider itself will be destroyed."
        )]
        public bool DestroyAttachedRigidbody = true;

        [Tooltip(
            "If non-null, this value will be used to filter OnTriggerEnter events to only those where the attached Rigidbody of " +
            $"the enterring Collider MATCHES or DOES NOT MATCH this Tag, depending on the value of {nameof(DestroyZone.FilterIsBlacklist)}."
        )]
        public string? AttachedRigidbodyTagFilter = null;

        [Tooltip(
            $"If true, then the {nameof(DestroyZone.AttachedRigidbodyTagFilter)} will be used as a blacklist " +
            $"(i.e., any enterring Collider will raise the UnityEvent EXCEPT those with an attached Rigidbody matching that Tag); " +
            $"if false, then {nameof(DestroyZone.AttachedRigidbodyTagFilter)} will be used as whitelist " +
            $"(i.e., only enterring Colliders with attached Rigidbodies MATCHING the filter will raise the event.)"
        )]
        public bool FilterIsBlacklist = false;
        public UnityEvent SomethingDestroyed = new();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnTriggerEnter(Collider other)
        {
            // Destroy the triggering Collider's GameObject, if requested
            if (!DestroyAttachedRigidbody) {
                Destroy(other.gameObject);
                SomethingDestroyed.Invoke();
                return;
            }

            // Otherwise, destroy the GameObject of the attached Rigidbody
            Rigidbody rb = other.attachedRigidbody;
            bool doDestroy = rb != null && (
                string.IsNullOrEmpty(AttachedRigidbodyTagFilter) ||
                (FilterIsBlacklist && !rb.CompareTag(AttachedRigidbodyTagFilter)) ||
                (!FilterIsBlacklist && rb.CompareTag(AttachedRigidbodyTagFilter))
            );
            if (doDestroy) {
                Destroy(rb!.gameObject);
                SomethingDestroyed.Invoke();
            }
        }

    }

}
