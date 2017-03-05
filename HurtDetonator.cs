using UnityEngine;

using System.Linq;
using System.Collections.Generic;

namespace Danware.Unity {

    public class HurtDetonator : MonoBehaviour {
        // INSPECTOR FIELDS
        public Detonator Detonator;
        public float MaxHealthDamage;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            Detonator.Detonated += (sender, e) =>
                e.Callback += affectTarget;
        }

        // HELPER FUNCTIONS
        private void affectTarget(Collider[] colliders) {
            // Damage all unique Health objects from these Colliders
            // Damage amount decreases with distance from the explosion
            Vector3 detonatorPos = Detonator.transform.position;
            colliders.SelectNonNull(c => c.GetComponent<PhysTarget>()?.TargetComponent as Health)
                     .Distinct()
                     .DoWith(h => {
                         float dist = Vector3.Distance(h.transform.position, detonatorPos);
                         float factor = 1f - Mathf.Min(1f, dist / Detonator.ExplosionRadius);
                         h.Damage(factor * MaxHealthDamage, HealthChangeMode);
                     });
        }
    }

}
