using UnityEngine;

using System.Collections.Generic;

namespace Danware.Unity3D {

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
            // Damage any Health component (damage amount decreases with distance from the explosion)
            HashSet<Health> healths = new HashSet<Health>();
            foreach (Collider c in colliders) {
                Health h = c.GetComponent<Health>();
                if (h != null)
                    healths.Add(h);
            }
            foreach (Health h in healths) {
                float dist = Vector3.Distance(Detonator.transform.position, h.transform.position);
                float hp = MaxHealthDamage * dist / Detonator.ExplosionRadius;
                h.Damage(hp, HealthChangeMode);
            }
        }
    }

}
