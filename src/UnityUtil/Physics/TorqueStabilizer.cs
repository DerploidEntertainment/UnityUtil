using System;

namespace UnityEngine {

    public class TorqueStabilizer : MonoBehaviour {

        // INSPECTOR FIELDS
        [Tooltip("The Rigidbody to which the stabilizing torque will be applied.")]
        public Rigidbody RigidbodyToStabilize;
        [Tooltip("The maximum torque that can be applied to stabilize the associated Rigidbody about the upward direction.  That is, if a larger torque than this is applied to the Rigidbody, this " + nameof(UnityEngine.TorqueStabilizer) + " will not be able to stabilize against it.")]
        public float MaxStabilizingTorque;
        [Tooltip("If the associated Rigidbody's angle of deflection from the upward direction is greater than this angle, then stabilizing torques will not be applied.  That is, beyond this deflection angle, the Rigidbody will just 'tip over'.")]
        [Range(0f, 180f)]
        public float MaxStabilizingAngle;
        [Tooltip("What axis should be considered upward?  That is, towards what axis will the stabilizing torque act to keep the associated Rigidbody upright?")]
        public AxisDirection UpwardDirectionType;
        [Tooltip("Only required if " + nameof(UpwardDirectionType) + " is " + nameof(AxisDirection.CustomWorldSpace) + " or " + nameof(AxisDirection.CustomLocalSpace) + ".")]
        public Vector3 CustomUpwardDirection;

        /// <summary>
        /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
        /// </summary>
        /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
        public Vector3 GetUpwardUnitVector() {
            switch (UpwardDirectionType) {
                case AxisDirection.WithGravity: return Physics.gravity.normalized;
                case AxisDirection.OppositeGravity: return -Physics.gravity.normalized;
                case AxisDirection.CustomWorldSpace: return CustomUpwardDirection.normalized;
                case AxisDirection.CustomLocalSpace: return RigidbodyToStabilize.transform.TransformDirection(CustomUpwardDirection.normalized);
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(UpwardDirectionType));
            }
        }

        // EVENT HANDLERS
        private void Reset() {
            MaxStabilizingTorque = 10f;
            MaxStabilizingAngle = 180f;
            UpwardDirectionType = AxisDirection.OppositeGravity;
            CustomUpwardDirection = Vector3.up;
        }
        private void OnDrawGizmos() {
            if (RigidbodyToStabilize != null)
                Gizmos.DrawLine(RigidbodyToStabilize.position, RigidbodyToStabilize.position + CustomUpwardDirection);
        }
        private void FixedUpdate() {
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

}
