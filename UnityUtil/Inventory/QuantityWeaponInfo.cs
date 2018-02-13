using UnityEngine;

namespace UnityUtil.Inventory {

    [CreateAssetMenu(fileName = "quantity-weapon", menuName = nameof(UnityUtil) + "/" + nameof(QuantityWeaponInfo))]
    public class QuantityWeaponInfo : ScriptableObject {

        [Tooltip("Attacked " + nameof(UnityUtil.ManagedQuantity) + "s will be changed by this amount.  How this amount is applied depends on the value of " + nameof(ChangeMode) + ".")]
        public float Amount = 10f;
        [Tooltip("Determines how the value of " + nameof(Amount) + " is used to change attacked " + nameof(UnityUtil.ManagedQuantity) + "s.")]
        public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;
        [Tooltip("If true, then only the closest " + nameof(UnityUtil.ManagedQuantity) + " attacked by this " + nameof(UnityUtil.Inventory.QuantityWeapon) + " will be changed.  If false, then all attacked " + nameof(UnityUtil.ManagedQuantity) + "s will be changed.")]
        public bool OnlyAffectClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

    }

}
