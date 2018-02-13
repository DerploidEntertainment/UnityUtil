using System.Linq;
using UnityEngine;

namespace UnityUtil {

    [RequireComponent(typeof(Detonator))]
    public class HurtDetonator : MonoBehaviour {

        private Detonator _detonator;

        // INSPECTOR FIELDS
        public float MaxHealthDamage;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            _detonator = GetComponent<Detonator>();
            _detonator.Detonated.AddListener(hurtAll);
        }

        // HELPER FUNCTIONS
        private void hurtAll(Collider[] colliders) {
            // Damage all unique Healths among these Colliders
            // Damage amount decreases linearly with distance from the explosion
            Health[] healths = colliders.Select(c => c.attachedRigidbody?.GetComponent<Health>())
                                        .Where(h => h != null)
                                        .Distinct()
                                        .ToArray();
            for (int h = 0; h < healths.Length; ++h) {
                Health health = healths[h];
                float dist = Vector3.Distance(health.transform.position, transform.position);
                float factor = 1f - Mathf.Min(1f, dist / _detonator.ExplosionRadius);
                health.Damage(factor * MaxHealthDamage, HealthChangeMode);
            }
        }
    }

}
