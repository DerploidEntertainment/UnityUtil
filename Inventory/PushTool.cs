using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class PushTool : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public PushToolInfo Info;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(Danware.Unity.Inventory.PushToolInfo)}!");

            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(push);
        }
        private void push(Vector3 direction, RaycastHit[] hits) {
            // If we should only push the closest Rigidbody, then scan for the Rigidbody to push
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, push the Rigidbodies on all Colliders that are not ignored with one of the specified tags
            RaycastHit[] newHits = (Info.OnlyPushClosest && hits.Length > 0) ? hits.OrderBy(h => h.distance).ToArray() : hits;
            for (int h = 0; h < newHits.Length; ++h) {
                RaycastHit hit = newHits[h];
                if (!Info.IgnoreColliderTags.Contains(hit.collider.tag)) {
                    Rigidbody rb = hit.collider.attachedRigidbody;
                    if (rb != null) {
                        rb.AddForceAtPosition(Info.PushForce * direction, hit.point, ForceMode.Impulse);
                        if (Info.OnlyPushClosest && hits.Length > 0)
                            break;
                    }
                }
            }
        }

    }

}
