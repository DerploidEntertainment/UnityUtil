using UnityEngine;

using System.Collections.Generic;

namespace Danware.Unity {

    public class HurtDetonator : MonoBehaviour {
        // INSPECTOR FIELDS
        public Detonator Detonator;
        public float MaxHealthDamage;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            Detonator.Detonated += (sender, e) => {
                e.Callback += affectTarget;
            };
        }

        // HELPER FUNCTIONS
        private void affectTarget(Collider[] colliders) {
            // Get the unique Health from these Colliders (without using Linq!)
            HashSet<Health> healths = new HashSet<Health>();
            foreach (Collider c in colliders) {
                PhysTarget pt = c.GetComponent<PhysTarget>();
                Health h = pt.TargetComponent as Health;
                if (h != null)
                    healths.Add(h);
            }

            // Damage these Healths (damage amount decreases with distance from the explosion)
            Vector3 detonatorPos = Detonator.transform.position;
            foreach (Health h in healths) {
                float dist = Vector3.Distance(h.transform.position, detonatorPos);
                float factor = 1f - Mathf.Min(1f, dist / Detonator.ExplosionRadius);
                float hp = MaxHealthDamage * factor;
                h.Damage(hp, HealthChangeMode);
            }
        }
    }

}
