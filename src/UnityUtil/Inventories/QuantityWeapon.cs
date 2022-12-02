using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace UnityUtil.Inventories;

[RequireComponent(typeof(Weapon))]
public class QuantityWeapon : MonoBehaviour
{
    private Weapon? _weapon;

    [Required]
    public QuantityWeaponInfo? Info;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        _weapon = GetComponent<Weapon>();
        _weapon.Attacked.AddListener(decreaseQuantity);
    }
    private void decreaseQuantity(Ray ray, RaycastHit[] hits)
    {
        // If we should only decrease the closest Quantity, then scan for the Quantity to damage
        // through the hit Colliders in increasing order of distance, ignoring Colliders with the specified tags
        // Otherwise, damage the Quantities on all Colliders that are not ignored with one of the specified tags
        for (int h = 0; h < hits.Length; ++h) {
            RaycastHit hit = hits[h];
            if (!Info!.IgnoreColliderTags.Contains(hit.collider.tag)) {
                ManagedQuantity? quantity = hit.collider.attachedRigidbody?.GetComponent<ManagedQuantity>();
                if (quantity != null) {
                    quantity.Change(Info.Amount, Info.ChangeMode);
                    if (Info.OnlyAffectClosest && hits.Length > 0)
                        break;
                }
            }
        }
    }

}
