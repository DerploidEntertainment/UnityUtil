using System.Linq;
using UnityEngine;

namespace UnityEngine {

    [RequireComponent(typeof(Detonator))]
    public class HurtDetonator : MonoBehaviour {

        private Detonator _detonator;

        // INSPECTOR FIELDS
        [Tooltip("Nearby " + nameof(UnityEngine.ManagedQuantity) + "s will be changed by this amount, at most.  How this amount is applied depends on the value of " + nameof(ChangeMode) + ", and the distance from this " + nameof(UnityEngine.Detonator) + ".")]
        public float MaxAmount = 10f;
        [Tooltip("Determines how the value of " + nameof(MaxAmount) + " is used to change nearby " + nameof(UnityEngine.ManagedQuantity) + "s.")]
        public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            _detonator = GetComponent<Detonator>();
            _detonator.Detonated.AddListener(changeAll);
        }

        // HELPER FUNCTIONS
        private void changeAll(Collider[] colliders) {
            // Change all unique Quantities among these Colliders
            // Change amount decreases linearly with distance from the explosion
            ManagedQuantity[] quantities =
                colliders.Select(c => c.attachedRigidbody?.GetComponent<ManagedQuantity>())
                         .Where(h => h != null)
                         .Distinct()
                         .ToArray();
            for (int h = 0; h < quantities.Length; ++h) {
                ManagedQuantity health = quantities[h];
                float dist = Vector3.Distance(health.transform.position, transform.position);
                float factor = 1f - Mathf.Min(1f, dist / _detonator.ExplosionRadius);
                health.Change(factor * MaxAmount, ChangeMode);
            }
        }
    }

}
