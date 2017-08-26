using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Weapon))]
    public class HurtWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float Damage = 10f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        [Tooltip("If true, then only the closest Health attacked by this Weapon will be damaged.  If false, then all attacked Healths will be damaged.")]
        public bool OnlyHurtClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(hurt);
        }
        private void hurt(Vector3 direction, RaycastHit[] hits) {
            // If we should only damage the closest Health, then scan for the Health to damage
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, damage the Healths on all Colliders that are not ignored with one of the specified tags
            RaycastHit[] newHits = (OnlyHurtClosest && hits.Length > 0) ? hits.OrderBy(h => h.distance).ToArray() : hits;
            for (int h = 0; h < newHits.Length; ++h) {
                RaycastHit hit = newHits[h];
                if (!IgnoreColliderTags.Contains(hit.collider.tag)) {
                    Health health = hit.collider.attachedRigidbody?.GetComponent<Health>();
                    if (health != null) {
                        health.Damage(Damage, HealthChangeMode);
                        if (OnlyHurtClosest && hits.Length > 0)
                            break;
                    }
                }
            }
        }

    }

}
