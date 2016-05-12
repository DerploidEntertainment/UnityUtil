using UnityEngine;

using System.Collections.Generic;

namespace Danware.Unity {

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
            // Get the unique Rigidbodies from these Colliders (without using Linq!)
            HashSet<Rigidbody> rbs = new HashSet<Rigidbody>();
            Rigidbody thisRb = this.GetComponent<Rigidbody>();
            foreach (Collider c in colliders) {
                if (!c.isTrigger) {
                    Rigidbody rb = c.attachedRigidbody;
                    if (rb != null && rb != thisRb)
                        rbs.Add(rb);
                }
            }

            // Apply an explosion force (upwards modifier changes direction with Physics.Gravity)
            Vector3 down = Physics.gravity.normalized;
            Vector3 detonatorPos = Detonator.transform.position;
            Vector3 explosionPos = detonatorPos + ExplosionUpwardsModifier * down;
            foreach (Rigidbody rb in rbs)
                rb.AddExplosionForce(ExplosionForce, Detonator.transform.position, Detonator.ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Impulse);
        }
    }

}
