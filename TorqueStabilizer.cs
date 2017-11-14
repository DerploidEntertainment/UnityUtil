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

        // HIDDEN FIELDS
        private void Awake() {
            Assert.IsNotNull(RigidbodyToStabilize, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(RigidbodyToStabilize)}");
        }
        private void OnDrawGizmos() =>
            Gizmos.DrawLine(RigidbodyToStabilize.position, RigidbodyToStabilize.position + CustomUpwardDirection);
        private void FixedUpdate() {
            // Determine the upward direction
            Vector3 up = Vector3.zero;
            switch (UpwardDirectionType) {
                case UpwardDirectionType.OppositeGravity: up = -Physics.gravity.normalized; break;
                case UpwardDirectionType.TransformUp: up = transform.up; break;
                case UpwardDirectionType.Custom: up = CustomUpwardDirection.normalized; break;
            }

            // Apply a torque to stabilize the Rigidbody that scales inversely with the angle of deflection
            float angle = Vector3.Angle(RigidbodyToStabilize.transform.up, up);
            float mag = Mathf.Max(MaxStabilizingTorque * angle / MaxStabilizingAngle, 0f);
            var dir = Vector3.Cross(RigidbodyToStabilize.transform.up, up);
            Vector3 torque = mag * dir;
            RigidbodyToStabilize.AddTorque(torque, ForceMode.Acceleration);
        }

    }

}
