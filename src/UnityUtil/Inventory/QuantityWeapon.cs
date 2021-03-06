﻿using System.Linq;
using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class QuantityWeapon : MonoBehaviour {

        private Weapon _weapon;

        // INSPECTOR FIELDS
        public QuantityWeaponInfo Info;

        // EVENT HANDLERS
        private void Awake() {
            this.AssertAssociation(Info, nameof(QuantityWeaponInfo));

            _weapon = GetComponent<Weapon>();
            _weapon.Attacked.AddListener(decreaseQuantity);
        }
        private void decreaseQuantity(Ray ray, RaycastHit[] hits) {
            // If we should only decrease the closest Quantity, then scan for the Quantity to damage
            // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
            // Otherwise, damage the Quantities on all Colliders that are not ignored with one of the specified tags
            for (int h = 0; h < hits.Length; ++h) {
                RaycastHit hit = hits[h];
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
