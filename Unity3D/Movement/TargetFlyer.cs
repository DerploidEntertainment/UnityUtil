using UnityEngine;

namespace Danware.Unity3D.Movement {

    [RequireComponent(typeof(Rigidbody))]
    public class TargetFlyer : MonoBehaviour {
        // ENCAPSULATED FIELDS
        private Rigidbody _rigidbody;

        // INSPECTOR FIELDS
        public float MoveSpeed = 20f;
        public float MoveAccel = 35f;
        public bool RotateClockWise = true;
        public float RotateSpeed = 5f;
        public float RotateAccel = 5f;
        public Vector3 Target;

        // UNITY FUNCTIONS
        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
        }
        private void FixedUpdate() {
            // Move/rotate towards the target vector
            flyRotate(Target);
            flyMove(Target);
        }

        // HELPER FUNCTIONS
        private void flyMove(Vector3 targetPosition) {
            // Add a Force to move towards the target position at constant velocity
            Vector3 toward = (targetPosition - transform.position).normalized;
            Vector3 vToward = Vector3.Project(_rigidbody.velocity, toward);
            float factor = (vToward.normalized == toward) ? Mathf.Sign(MoveSpeed * MoveSpeed - vToward.sqrMagnitude) : 1;
            _rigidbody.AddForce(factor * MoveAccel * toward, ForceMode.Acceleration);

            // Add a Force to reduce any velocity in the normal direction
            Vector3 vNorm = Vector3.ProjectOnPlane(_rigidbody.velocity, toward);
            Vector3 norm = vNorm.normalized;
            if (vNorm.sqrMagnitude > 0f)
                _rigidbody.AddForce(-MoveAccel * norm, ForceMode.Acceleration);
        }
        private void flyRotate(Vector3 targetPosition) {
            // Add a Force to rotate around the target direction at constant angular velocity
            Vector3 toward = (targetPosition - transform.position).normalized;
            Vector3 wToward = Vector3.Project(_rigidbody.angularVelocity, toward);
            float factor = (wToward.normalized == toward) ? Mathf.Sign(RotateSpeed * RotateSpeed - wToward.sqrMagnitude) : 1;
            factor *= (RotateClockWise ? 1 : -1);
            _rigidbody.AddTorque(factor * RotateAccel * toward, ForceMode.Acceleration);

            // Add a Force to reduce any angular velocity around the normal direction
            Vector3 wNorm = Vector3.ProjectOnPlane(_rigidbody.angularVelocity, toward);
            Vector3 norm = wNorm.normalized;
            if (wNorm.sqrMagnitude > 0f)
                _rigidbody.AddTorque(-RotateAccel * norm, ForceMode.Acceleration);
        }
    }

}
