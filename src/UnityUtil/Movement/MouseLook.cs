using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Physics;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Movement;

public class MouseLook : Updatable
{
    private float _angle;
    private float _deltaSinceLast;

    [Tooltip($"The Transform that will be kinematically rotated while looking around.  Only required if {nameof(UsePhysicsToLook)} is false.")]
    public Transform? TransformToRotate;

    [Tooltip($"The Rigidbody that will be rotated by physics while looking around.  Only required if {nameof(UsePhysicsToLook)} is true.")]
    public Rigidbody? RigidbodyToRotate;

    [Required]
    public ValueInput? LookInput;

    [Tooltip(
        "The maximum angle around the vertical that can be looked through in the positive direction. " +
        "For best results, use values less than 180° to limit the view, or exactly 360° to allow full rotation."
    )]
    [Range(0f, 360f)]
    public float MaxPositiveAngle = 360f;

    [Tooltip(
        "The maximum angle around the vertical that can be looked through in the negative direction. " +
        "For best results, use values greater than -180° to limit the view, or exactly -360° to allow full rotation."
    )]
    [Range(-360f, 0f)]
    public float MaxNegativeAngle = -360f;

    [Tooltip("If true, then the look rotation is applied using physics, otherwise it is applied using kinematic Transform rotation.")]
    public bool UsePhysicsToLook = true;

    [Tooltip("Around what axis will the look rotation be applied?")]
    public AxisDirection AxisDirectionType = AxisDirection.OppositeGravity;

    [Tooltip($"Only required if {nameof(AxisDirectionType)} is {nameof(AxisDirection.CustomWorldSpace)} or {nameof(AxisDirection.CustomLocalSpace)}.")]
    public Vector3 CustomAxisDirection = Vector3.up;

    public Vector3 GetUpwardUnitVector(Transform relativeTransform) =>
        AxisDirectionType switch {
            AxisDirection.WithGravity => U.Physics.gravity.normalized,
            AxisDirection.OppositeGravity => -U.Physics.gravity.normalized,
            AxisDirection.CustomWorldSpace => CustomAxisDirection.normalized,
            AxisDirection.CustomLocalSpace => relativeTransform.TransformDirection(CustomAxisDirection.normalized),
            _ => throw UnityObjectExtensions.SwitchDefaultException(AxisDirectionType),
        };

    protected override void Awake()
    {
        base.Awake();

        UpdateAction = doUpdate;
        FixedUpdateAction = doFixedUpdate;
    }
    private void doUpdate(float deltaTime)
    {
        _deltaSinceLast += LookInput!.Value();

        if (!UsePhysicsToLook && TransformToRotate != null) {
            doLookRotation();
            _deltaSinceLast = 0f;
        }
    }
    private void doFixedUpdate(float fixedDeltaTime)
    {
        if (UsePhysicsToLook && RigidbodyToRotate != null) {
            doLookRotation();
            _deltaSinceLast = 0f;
        }
    }

    private void doLookRotation()
    {
        // Rotate the requested number of degrees around the upward axis, using the desired method
        float deltaAngle = (_deltaSinceLast > 0) ? Mathf.Min(MaxPositiveAngle - _angle, _deltaSinceLast) : Mathf.Max(MaxNegativeAngle - _angle, _deltaSinceLast);
        if (UsePhysicsToLook && RigidbodyToRotate != null) {
            Vector3 up = GetUpwardUnitVector(RigidbodyToRotate.transform);
            RigidbodyToRotate.MoveRotation(RigidbodyToRotate.rotation * Quaternion.AngleAxis(deltaAngle, up));
        }
        else if (!UsePhysicsToLook && TransformToRotate != null) {
            Vector3 up = GetUpwardUnitVector(TransformToRotate);
            TransformToRotate.Rotate(up, deltaAngle, Space.World);
        }

        // Adjust the internal angle counter
        _angle += deltaAngle;
        if (Mathf.Abs(_angle) >= 360f)
            _angle = 0f;
    }

}
