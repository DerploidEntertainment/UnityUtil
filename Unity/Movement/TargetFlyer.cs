using UnityEngine;

namespace Danware.Unity.Movement {
    
    public class TargetFlyer : MonoBehaviour {
        // INSPECTOR FIELDS
        public Rigidbody FlyingRigidbody;
        public float MoveSpeed = 20f;
        public float MoveAccel = 35f;
        public bool RotateClockWise = true;
        public float RotateSpeed = 5f;
        public float RotateAccel = 5f;
        public Vector3 Target;

        // UNITY FUNCTIONS
        private void FixedUpdate() {
            // Move/rotate towards the target vector
            if (FlyingRigidbody != null) {
                flyRotate(Target);
                flyMove(Target);
            }
        }

        // HELPER FUNCTIONS
        private void flyMove(Vector3 targetPosition) {
            // Add a Force to move towards the target position at constant velocity
            Vector3 toward = (targetPosition - transform.position).normalized;
            Vector3 vToward = Vector3.Project(FlyingRigidbody.velocity, toward);
            float factor = (vToward.normalized == toward) ? Mathf.Sign(MoveSpeed * MoveSpeed - vToward.sqrMagnitude) : 1;
            FlyingRigidbody.AddForce(factor * MoveAccel * toward, ForceMode.Acceleration);

            // Add a Force to reduce any velocity in the normal direction
            Vector3 vNorm = Vector3.ProjectOnPlane(FlyingRigidbody.velocity, toward);
            Vector3 norm = vNorm.normalized;
            if (vNorm.sqrMagnitude > 0f)
                FlyingRigidbody.AddForce(-MoveAccel * norm, ForceMode.Acceleration);
        }
        private void flyRotate(Vector3 targetPosition) {
            // Add a Force to rotate around the target direction at constant angular velocity
            Vector3 toward = (targetPosition - transform.position).normalized;
            Vector3 wToward = Vector3.Project(FlyingRigidbody.angularVelocity, toward);
            float factor = (wToward.normalized == toward) ? Mathf.Sign(RotateSpeed * RotateSpeed - wToward.sqrMagnitude) : 1;
            factor *= (RotateClockWise ? 1 : -1);
            FlyingRigidbody.AddTorque(factor * RotateAccel * toward, ForceMode.Acceleration);

            // Add a Force to reduce any angular velocity around the normal direction
            Vector3 wNorm = Vector3.ProjectOnPlane(FlyingRigidbody.angularVelocity, toward);
            Vector3 norm = wNorm.normalized;
            if (wNorm.sqrMagnitude > 0f)
                FlyingRigidbody.AddTorque(-RotateAccel * norm, ForceMode.Acceleration);
        }
    }

}
