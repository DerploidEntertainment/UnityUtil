namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "weapon", menuName = nameof(UnityEngine.Inventory) + "/" + nameof(UnityEngine.Inventory.WeaponInfo))]
    public class WeaponInfo : ScriptableObject {

        [Tooltip("Only colliders matching this layer mask will be attacked.")]
        public LayerMask AttackLayerMask;
        [Tooltip("Only colliders within this range will be attacked.")]
        public float Range;

        [Header("Accuracy")]
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(InitialConeHalfAngle) + " to " + nameof(FinalConeHalfAngle) + " in " + nameof(AccuracyLerpTime) + " seconds.")]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(InitialConeHalfAngle) + " to " + nameof(FinalConeHalfAngle) + " in " + nameof(AccuracyLerpTime) + " seconds.")]
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;
        [Tooltip("For automatic " + nameof(UnityEngine.Inventory.Weapon) + "s, the accuracy cone's half angle will interpolate linearly from " + nameof(InitialConeHalfAngle) + " to " + nameof(FinalConeHalfAngle) + " in " + nameof(AccuracyLerpTime) + " seconds.")]
        public float AccuracyLerpTime = 1f;
        [Tooltip("If true, then all colliders within " + nameof(Range) + " and on the " + nameof(AttackLayerMask) + " will be attacked, using the relatively expensive Physics.RaycastAll() method.  If false, only the closest collider will be attacked, using the cheaper Physics.Raycast() method.")]
        public bool RaycastAll = false;

    }

}
