using UnityEngine;

using System.Collections.Generic;

namespace Danware.Unity3D {

    public class PushDetonator : MonoBehaviour {
        // INSPECTOR FIELDS
        public Detonator Detonator;
        public float ExplosionForce = 10f;
        public float ExplosionUpwardsModifier = 2f;

        // EVENT HANDLERS
        private void Awake() {
            Detonator.Detonated += (sender, e) => {
                e.Callback += affectTarget;
            };
        }

        // HELPER FUNCTIONS
        private void affectTarget(Collider[] colliders) {
            // Apply an explosion force
            HashSet<Rigidbody> rbs = new HashSet<Rigidbody>();
            foreach (Collider c in colliders) {
                if (!c.isTrigger) {
                    Rigidbody rb = c.attachedRigidbody;
                    if (rb != null)
                        rbs.Add(rb);
                }
            }
            foreach (Rigidbody rb in rbs)
                rb.AddExplosionForce(ExplosionForce, Detonator.transform.position, Detonator.ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Impulse);
        }
    }

}
