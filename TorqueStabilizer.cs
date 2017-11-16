using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity {

    public class TorqueStabilizer : MonoBehaviour {

        // INSPECTOR FIELDS
        public Rigidbody RigidbodyToStabilize;
        public float MaxStabilizingTorque = 10f;
        public float MaxStabilizingAngle = 45f;
        public UpwardDirectionType UpwardDirectionType = UpwardDirectionType.OppositeGravity;
        public Vector3 CustomUpwardDirection = Vector3.up;

        /// <summary>
        /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
        /// </summary>
        /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
        public Vector3 GetUpwardUnitVector() {
            switch (UpwardDirectionType) {
                case AxisDirection.WithGravity: return Physics.gravity.normalized;
                case AxisDirection.OppositeGravity: return -Physics.gravity.normalized;
                case AxisDirection.CustomWorldSpace: return CustomUpwardDirection.normalized;
                case AxisDirection.CustomLocalSpace: return transform.TransformDirection(CustomUpwardDirection.normalized);
                default: throw new NotImplementedException($"Gah!  We haven't accounted for {nameof(Danware.Unity.AxisDirection)} {UpwardDirectionType}!");
            }
        }

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(RigidbodyToStabilize, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(RigidbodyToStabilize)}");
        }
        private void OnDrawGizmos() =>
            Gizmos.DrawLine(RigidbodyToStabilize.position, RigidbodyToStabilize.position + CustomUpwardDirection);
        private void FixedUpdate() {
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
