using UnityEngine;

namespace UnityUtil.Inventory {

    [CreateAssetMenu(fileName = "hurt-weapon", menuName = "UnityUtil/HurtWeaponInfo")]
    public class HurtWeaponInfo : ScriptableObject {

        [Tooltip("Attacked " + nameof(UnityUtil.Health) + "s will be damaged by this amount.  How this amount is applied depends on the value of " + nameof(HealthChangeMode) + ".")]
        public float Damage = 10f;
        [Tooltip("Determines how the value of " + nameof(Damage) + " is used to damage attacked " + nameof(UnityUtil.Health) + "s.")]
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        [Tooltip("If true, then only the closest " + nameof(UnityUtil.Health) + " attacked by this " + nameof(UnityUtil.Inventory.HurtWeapon) + " will be damaged.  If false, then all attacked " + nameof(UnityUtil.Health) + "s will be damaged.")]
        public bool OnlyHurtClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

    }

}
