using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class HurtWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public HurtWeaponInfo Info;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, this.GetAssociationAssertion(nameof(Danware.Unity.Inventory.HurtWeaponInfo)));

            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(hurt);
        }
        private void hurt(Vector3 direction, RaycastHit[] hits) {
            // If we should only damage the closest Health, then scan for the Health to damage
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, damage the Healths on all Colliders that are not ignored with one of the specified tags
            RaycastHit[] newHits = (Info.OnlyHurtClosest && hits.Length > 0) ? hits.OrderBy(h => h.distance).ToArray() : hits;
            for (int h = 0; h < newHits.Length; ++h) {
                RaycastHit hit = newHits[h];
                if (!Info.IgnoreColliderTags.Contains(hit.collider.tag)) {
                    Health health = hit.collider.attachedRigidbody?.GetComponent<Health>();
                    if (health != null) {
                        health.Damage(Info.Damage, Info.HealthChangeMode);
                        if (Info.OnlyHurtClosest && hits.Length > 0)
                            break;
                    }
                }
            }
        }

    }

}
