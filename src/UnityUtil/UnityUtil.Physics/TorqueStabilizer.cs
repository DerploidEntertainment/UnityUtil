using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Physics;

public class TorqueStabilizer : MonoBehaviour
{
    [Tooltip("The Rigidbody to which the stabilizing torque will be applied.")]
    public Rigidbody? RigidbodyToStabilize;

    [Tooltip(
        "The maximum torque that can be applied to stabilize the associated Rigidbody about the upward direction. " +
        $"That is, if a larger torque than this is applied to the Rigidbody, this {nameof(TorqueStabilizer)} " +
        "will not be able to stabilize against it."
    )]
    [Min(0f)]
    public float MaxStabilizingTorque = 10f;

    [Tooltip(
        "If the associated Rigidbody's angle of deflection from the upward direction is greater than this angle, " +
        "then stabilizing torques will not be applied. That is, beyond this deflection angle, the Rigidbody will just 'tip over'."
    )]
    [Range(0f, 180f)]
    public float MaxStabilizingAngle = 180f;

    [Tooltip(
        "What axis should be considered upward? " +
        $"That is, toward what axis will the stabilizing torque act to keep {nameof(RigidbodyToStabilize)} upright?"
    )]
    public AxisDirection UpwardDirectionType = AxisDirection.OppositeGravity;

    [Tooltip($"Only required if {nameof(UpwardDirectionType)} is {nameof(AxisDirection.CustomWorldSpace)} or {nameof(AxisDirection.CustomLocalSpace)}.")]
    public Vector3 CustomUpwardDirection = Vector3.up;

    /// <summary>
    /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
    /// </summary>
    /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
    public Vector3 GetUpwardUnitVector() =>
        UpwardDirectionType switch {
            AxisDirection.WithGravity => U.Physics.gravity.normalized,
            AxisDirection.OppositeGravity => -U.Physics.gravity.normalized,
            AxisDirection.CustomWorldSpace => CustomUpwardDirection.normalized,
            AxisDirection.CustomLocalSpace => RigidbodyToStabilize!.transform.TransformDirection(CustomUpwardDirection.normalized),
            _ => throw UnityObjectExtensions.SwitchDefaultException(UpwardDirectionType),
        };

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnDrawGizmos()
    {
        if (RigidbodyToStabilize != null)
            Gizmos.DrawLine(RigidbodyToStabilize.position, RigidbodyToStabilize.position + CustomUpwardDirection);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void FixedUpdate()
    {
        if (RigidbodyToStabilize == null)
            return;

        // Determine the upward direction
        Vector3 up = GetUpwardUnitVector();

        // Apply a torque to stabilize the Rigidbody that scales inversely with the angle of deflection
        float angle = Vector3.Angle(RigidbodyToStabilize.transform.up, up);
        float mag = Mathf.Max(MaxStabilizingTorque * angle / MaxStabilizingAngle, 0f);
        var dir = Vector3.Cross(RigidbodyToStabilize.transform.up, up);
        Vector3 torque = mag * dir;
        RigidbodyToStabilize.AddTorque(torque, ForceMode.Acceleration);
    }

}
