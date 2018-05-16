namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "quantity-weapon", menuName = nameof(UnityEngine.Inventory) + "/" + nameof(UnityEngine.Inventory.QuantityWeaponInfo))]
    public class QuantityWeaponInfo : ScriptableObject {

        [Tooltip("Attacked " + nameof(UnityEngine.ManagedQuantity) + "s will be changed by this amount.  How this amount is applied depends on the value of " + nameof(ChangeMode) + ".")]
        public float Amount = 10f;
        [Tooltip("Determines how the value of " + nameof(Amount) + " is used to change attacked " + nameof(UnityEngine.ManagedQuantity) + "s.")]
        public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;
        [Tooltip("If true, then only the closest " + nameof(UnityEngine.ManagedQuantity) + " attacked by this " + nameof(UnityEngine.Inventory.QuantityWeapon) + " will be changed.  If false, then all attacked " + nameof(UnityEngine.ManagedQuantity) + "s will be changed.")]
        public bool OnlyAffectClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

    }

}
