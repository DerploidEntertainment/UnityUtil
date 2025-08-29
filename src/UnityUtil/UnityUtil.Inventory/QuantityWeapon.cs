using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Weapon))]
public class QuantityWeapon : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public QuantityWeaponInfo? Info;

    private void Awake()
    {
        Weapon? weapon = GetComponent<Weapon>();
        weapon.Attacked.AddListener(decreaseQuantity);
    }
    private void decreaseQuantity(Ray ray, RaycastHit[] hits)
    {
        // If we should only decrease the closest Quantity, then scan for the Quantity to damage
        // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
        // Otherwise, damage the Quantities on all Colliders that are not ignored with one of the specified tags
        for (int h = 0; h < hits.Length; ++h) {
            RaycastHit hit = hits[h];
            if (!Info!.IgnoreColliderTags.Contains(hit.collider.tag)
                && hit.collider.attachedRigidbody != null
                && hit.collider.attachedRigidbody.TryGetComponent(out ManagedQuantity quantity)
            ) {
                quantity.Change(Info.Amount, Info.ChangeMode);
                if (Info.OnlyAffectClosest && hits.Length > 0)
                    break;
            }
        }
    }

}
