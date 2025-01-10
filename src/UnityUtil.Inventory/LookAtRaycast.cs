using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityUtil.Inventory;
using UnityUtil.Physics;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Movement;

public class LookAtRaycast : Updatable
{

    [Tooltip(
        $"This Transform will always rotate to look at whatever the {nameof(RaycastingTransform)} is looking at. " +
        $"This point is at most {nameof(Range)} units ahead, but will be closer if an object matching {nameof(LayerMask)} is closer."
    )]
    public Transform? TransformToRotate;

    private const string TOOLTIP_TRANSFORM_ROTATE =
        $"The {nameof(TransformToRotate)} will always rotate to look at whatever the {nameof(RaycastingTransform)} is looking at. " +
        $"This point is at most {nameof(Range)} units ahead, but will be closer if an object matching {nameof(LayerMask)} is closer.";
    private const string TOOLTIP_WEAPONINFO = $"If a {nameof(WeaponInfo)} is provided, then its values will be given priority over this value.";


    [Tooltip(TOOLTIP_TRANSFORM_ROTATE)]
    public Transform? RaycastingTransform;

    [Tooltip($"{TOOLTIP_TRANSFORM_ROTATE} {TOOLTIP_WEAPONINFO}")]
    public float Range;

    [Tooltip($"{TOOLTIP_TRANSFORM_ROTATE} {TOOLTIP_WEAPONINFO}")]
    public LayerMask LayerMask;

    [Tooltip(
        $"If the {nameof(RaycastingTransform)} is associated with a {nameof(Weapon)}, " +
        $"then providing its {nameof(Inventory.WeaponInfo)} here will override {nameof(Range)} and {nameof(LayerMask)}, " +
        $"which might be less error-prone during development."
    )]
    public WeaponInfo? WeaponInfo;

    [Tooltip(
        $"This upward direction will be used by the {nameof(TransformToRotate)} to rotate toward " +
        $"whatever the {nameof(RaycastingTransform)} is looking at."
    )]
    public AxisDirection UpwardDirectionType = AxisDirection.OppositeGravity;

    [Tooltip($"Only required if {nameof(UpwardDirectionType)} is {nameof(AxisDirection.CustomWorldSpace)} or {nameof(AxisDirection.CustomLocalSpace)}.")]
    public Vector3 CustomUpwardDirection = Vector3.up;

    /// <summary>
    /// Returns the unit vector that this <see cref="LookAtRaycast"/> will use to rotate towards what its associated <see cref="RaycastingTransform"/> is looking at.
    /// </summary>
    /// <returns>The unit vector that this <see cref="LookAtRaycast"/> will use to rotate towards what its associated <see cref="RaycastingTransform"/> is looking at.</returns>
    public Vector3 GetUpwardUnitVector() =>
        UpwardDirectionType switch {
            AxisDirection.WithGravity => U.Physics.gravity.normalized,
            AxisDirection.OppositeGravity => -U.Physics.gravity.normalized,
            AxisDirection.CustomWorldSpace => CustomUpwardDirection.normalized,
            AxisDirection.CustomLocalSpace => TransformToRotate!.TransformDirection(CustomUpwardDirection.normalized),
            _ => throw UnityObjectExtensions.SwitchDefaultException(UpwardDirectionType),
        };

    protected override void Awake()
    {
        base.Awake();

        AddUpdate(doUpdate);
    }
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnDrawGizmos()
    {
        if (TransformToRotate == null)
            return;

        float range = WeaponInfo?.Range ?? Range;
        Gizmos.DrawLine(TransformToRotate.position, TransformToRotate.TransformPoint(range * Vector3.forward));
    }
    private void doUpdate(float deltaTime)
    {
        if (RaycastingTransform == null || TransformToRotate == null)
            return;

        // Determine the point that the raycasting transform is looking at.
        // May be a point on an actual collider up ahead, or just a point out at its max range.
        float range = WeaponInfo?.Range ?? Range;
        LayerMask layerMask = WeaponInfo?.AttackLayerMask ?? LayerMask;
        bool somethingHit = U.Physics.Raycast(RaycastingTransform.position, RaycastingTransform.forward, out RaycastHit hitInfo, range, layerMask);
        Vector3 targetPos = somethingHit ? hitInfo.point : RaycastingTransform.TransformPoint(range * Vector3.forward);

        // Look at that point using the specified UpwardDirectionType
        TransformToRotate.LookAt(targetPos, GetUpwardUnitVector());
    }

}
