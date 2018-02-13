using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class QuantityWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public QuantityWeaponInfo Info;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, this.GetAssociationAssertion(nameof(UnityUtil.Inventory.QuantityWeaponInfo)));

            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(decreaseQuantity);
        }
        private void decreaseQuantity(Vector3 direction, RaycastHit[] hits) {
            // If we should only decrease the closest Quantity, then scan for the Quantity to damage
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, damage the Quantities on all Colliders that are not ignored with one of the specified tags
            RaycastHit[] newHits = (Info.OnlyAffectClosest && hits.Length > 0) ? hits.OrderBy(h => h.distance).ToArray() : hits;
            for (int h = 0; h < newHits.Length; ++h) {
                RaycastHit hit = newHits[h];
                if (!Info.IgnoreColliderTags.Contains(hit.collider.tag)) {
                    ManagedQuantity quantity = hit.collider.attachedRigidbody?.GetComponent<ManagedQuantity>();
                    if (quantity != null) {
                        quantity.Change(Info.Amount, Info.ChangeMode);
                        if (Info.OnlyAffectClosest && hits.Length > 0)
                            break;
                    }
                }
            }
        }

    }

}
