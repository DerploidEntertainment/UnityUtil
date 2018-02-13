using System.Linq;
using UnityEngine;

namespace UnityUtil {

    [RequireComponent(typeof(Detonator))]
    public class PushDetonator : MonoBehaviour {

        private Detonator _detonator;

        // INSPECTOR FIELDS
        public float ExplosionForce = 10f;
        public float ExplosionUpwardsModifier = 2f;

        // EVENT HANDLERS
        private void Awake() {
            _detonator = GetComponent<Detonator>();
            _detonator.Detonated.AddListener(pushAll);
        }

        // HELPER FUNCTIONS
        private void pushAll(Collider[] colliders) {
            // Apply an explosion force to all unique Rigidbodies among these Colliders
            // Upwards modifier adjusts to gravity
            Rigidbody[] rigidbodies = colliders.Select(c => c.attachedRigidbody)
                                        .Where(rb => rb != null)
                                        .Distinct()
                                        .ToArray();
            for (int rb = 0; rb < rigidbodies.Length; ++rb) {
                Rigidbody rigidbody = rigidbodies[rb];
                Vector3 explosionPos = _detonator.transform.position + ExplosionUpwardsModifier * Physics.gravity.normalized;
                rigidbody.AddExplosionForce(ExplosionForce, explosionPos, _detonator.ExplosionRadius, 0f, ForceMode.Impulse);
            }
        }

    }

}
