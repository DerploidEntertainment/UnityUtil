using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Weapon))]
    public class HurtWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float Damage = 10f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        [Tooltip("If true, then only the closest Health attacked by the BaseWeapon will be damaged.  If false, then all attacked Healths will be damaged.")]
        public bool OnlyHurtClosest = false;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(hurt);
        }
        private void hurt(Vector3 direction, RaycastHit[] hits) {
            // Hurt the associated Healths of all hit colliders (or of the closest one only, if requested)
            if (OnlyHurtClosest && hits.Length > 0) {
                Health health = hits.OrderBy(h => h.distance).First().collider.attachedRigidbody?.GetComponent<Health>();
                health?.Damage(Damage, HealthChangeMode);
            }
            else {
                for (int h = 0; h < hits.Length; ++h) {
                    Health health = hits[h].collider.attachedRigidbody?.GetComponent<Health>();
                    health?.Damage(Damage, HealthChangeMode);
                }
            }
        }

    }

}
