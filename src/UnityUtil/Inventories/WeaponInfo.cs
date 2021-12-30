namespace UnityEngine.Inventories {

    public enum PhysicsCastShape {
        Ray,
        Box,
        Sphere,
        Capsule
    }

    [CreateAssetMenu(fileName = "weapon", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inventories)}/{nameof(UnityEngine.Inventories.WeaponInfo)}")]
    public class WeaponInfo : ScriptableObject {

        [Tooltip("Only colliders matching this layer mask will be attacked.")]
        public LayerMask AttackLayerMask;

        [Tooltip("Only colliders within this range will be attacked.")]
        public float Range;

        [Tooltip(
            $"If true, then all colliders within {nameof(WeaponInfo.Range)} and on the {nameof(WeaponInfo.AttackLayerMask)} will be attacked " +
            $"(using the relatively expensive Physics.RaycastAll() method). " +
            $"If false, then only {nameof(WeaponInfo.MaxAttacks)} colliders will be attacked."
        )]
        public bool AttackAllInRange = false;

        [Tooltip(
            $"The maximum number of colliders within {nameof(WeaponInfo.Range)} and on the {nameof(WeaponInfo.AttackLayerMask)} to attack. " +
            $"If this value is 1, then Physics.Raycast() will be used to find colliders to attack, otherwise the relatively expensive " +
            $"Physics.RaycastAll() will be used (with only the {nameof(WeaponInfo.MaxAttacks)} closest colliders actually being attacked)."
        )]
        [Min(1f)]
        public uint MaxAttacks = 1u;

        [Tooltip($"Determines the shape of the 'shot' or 'blast' created by associated {nameof(UnityEngine.Inventories.Weapon)}s.")]
        public PhysicsCastShape PhysicsCastShape = PhysicsCastShape.Ray;


        [Header("BoxCast")]

        [Tooltip("Half the size of the box in each dimension.")]
        public Vector3 HalfExtents = 0.5f * Vector3.one;

        [Tooltip("Rotation of the box.")]
        public Quaternion Orientation = Quaternion.identity;


        [Header("SphereCast and CapsuleCast")]

        [Tooltip(
            "The radius of the sphere/capsule. " +
            "Note that Sphere/CapsuleCast will not detect colliders for which the sphere/capsule overlaps the collider. " +
            "Passing a zero radius results in undefined output and doesn't always behave the same as Physics.Raycast."
        )]
        public float Radius = 0.5f;

        [Tooltip("The center of the sphere at the 'first' end of the capsule, in local coordinates.")]
        public Vector3 Point1 = 0.5f * Vector3.up;

        [Tooltip("The center of the sphere at the 'second' end of the capsule, in local coordinates.")]
        public Vector3 Point2 = 0.5f * Vector3.one;


        [Header("Accuracy")]

        private const string TOOLTIP_ACCURACY =
            $"For automatic {nameof(UnityEngine.Inventories.Weapon)}s, the accuracy cone's half angle will interpolate linearly " +
            $"from {nameof(WeaponInfo.InitialConeHalfAngle)} to {nameof(WeaponInfo.FinalConeHalfAngle)} in {nameof(WeaponInfo.AccuracyLerpTime)} seconds. " +
            $"Casts of all {nameof(WeaponInfo.PhysicsCastShape)}s will be performed along a random direction in this cone.";

        [Tooltip(TOOLTIP_ACCURACY)]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;

        [Tooltip(TOOLTIP_ACCURACY)]
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;

        [Tooltip(TOOLTIP_ACCURACY)]
        [Min(0f)]
        public float AccuracyLerpTime = 1f;

    }

}
