namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "weapon", menuName = nameof(UnityEngine.Inventory) + "/" + nameof(UnityEngine.Inventory.WeaponInfo))]
    public class WeaponInfo : ScriptableObject {

        [Tooltip("Only colliders matching this layer mask will be attacked.")]
        public LayerMask AttackLayerMask;
        [Tooltip("Only colliders within this range will be attacked.")]
        public float Range;

        [Header("Accuracy")]
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(WeaponInfo.InitialConeHalfAngle) + " to " + nameof(WeaponInfo.FinalConeHalfAngle) + " in " + nameof(WeaponInfo.AccuracyLerpTime) + " seconds.")]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(WeaponInfo.InitialConeHalfAngle) + " to " + nameof(WeaponInfo.FinalConeHalfAngle) + " in " + nameof(WeaponInfo.AccuracyLerpTime) + " seconds.")]
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(WeaponInfo.InitialConeHalfAngle) + " to " + nameof(WeaponInfo.FinalConeHalfAngle) + " in " + nameof(WeaponInfo.AccuracyLerpTime) + " seconds.")]
        public float AccuracyLerpTime = 1f;
        [Tooltip("If true, then all colliders within " + nameof(WeaponInfo.Range) + " and on the " + nameof(WeaponInfo.AttackLayerMask) + " will be attacked (using the relatively expensive Physics.RaycastAll() method)  If false, then only " + nameof(WeaponInfo.MaxAttacks) + " colliders will be attacked.")]
        public bool AttackAllInRange = false;
        [Tooltip("The maximum number of colliders within " + nameof(WeaponInfo.Range) + " and on the " + nameof(WeaponInfo.AttackLayerMask) + " to attack.  If this value is 1, then Physics.Raycast() will be used to find colliders to attack, otherwise the relatively expensive Physics.RaycastAll() will be used (with only the " + nameof(WeaponInfo.MaxAttacks) + " closest colliders actually being attacked).  This value can theoretically be zero, but that would make any associated " + nameof(UnityEngine.Inventory.Weapon) + " kind of pointless!")]
        public uint MaxAttacks = 1;

    }

}
