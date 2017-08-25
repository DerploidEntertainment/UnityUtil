using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class PushWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float PushForce = 1f;
        [Tooltip("If true, then only the closest collider attacked by the BaseWeapon will be pushed.  If false, then all attacked colliders will be pushed.")]
        public bool OnlyPushClosest = false;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(push);
        }
        private void push(Vector3 direction, RaycastHit[] hits) {
            // Apply a force to the associated Rigidbodies of all hit colliders (or of the closest one only, if requested)
            if (OnlyPushClosest && hits.Length > 0) {
                Rigidbody rb = hits.OrderBy(h => h.distance).First().collider.attachedRigidbody;
                rb?.AddForceAtPosition(PushForce * direction, hits[0].point, ForceMode.Impulse);
            }
            else {
                for (int h = 0; h < hits.Length; ++h) {
                    Rigidbody rb = hits[h].collider.attachedRigidbody;
                    rb?.AddForceAtPosition(PushForce * direction, hits[h].point, ForceMode.Impulse);
                }
            }
        }

    }

}
