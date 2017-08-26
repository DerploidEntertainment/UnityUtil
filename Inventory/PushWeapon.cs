using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class PushWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float PushForce = 1f;
        [Tooltip("If true, then only the closest Rigidbody attacked by this Weapon will be pushed.  If false, then all attacked Rigidbodies will be pushed.")]
        public bool OnlyPushClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(push);
        }
        private void push(Vector3 direction, RaycastHit[] hits) {
            // If we should only push the closest Rigidbody, then scan for the Rigidbody to push
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, push the Rigidbodies on all Colliders that are not ignored with one of the specified tags
            RaycastHit[] newHits = (OnlyPushClosest && hits.Length > 0) ? hits.OrderBy(h => h.distance).ToArray() : hits;
            for (int h = 0; h < newHits.Length; ++h) {
                RaycastHit hit = newHits[h];
                if (!IgnoreColliderTags.Contains(hit.collider.tag)) {
                    Rigidbody rb = hit.collider.attachedRigidbody;
                    if (rb != null) {
                        rb.AddForceAtPosition(PushForce * direction, hit.point, ForceMode.Impulse);
                        if (OnlyPushClosest && hits.Length > 0)
                            break;
                    }
                }
            }
        }

    }

}
