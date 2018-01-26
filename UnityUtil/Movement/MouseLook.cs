using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityUtil.Input;

namespace UnityUtil.Movement {

    public class MouseLook : MonoBehaviour {
        // HIDDEN FIELDS
        private float _angle = 0f;
        private float _deltaSinceLast = 0f;

        // INSPECTOR FIELDS
        [Tooltip("The Transform that will be kinematically rotated while looking around.  Only required if " + nameof(UsePhysicsToLook) + " is false.")]
        public Transform TransformToRotate;
        [Tooltip("The Rigidbody that will be rotated by physics while looking around.  Only required if " + nameof(UsePhysicsToLook) + " is true.")]
        public Rigidbody RigidbodyToRotate;
        public ValueInput LookInput;
        [Tooltip("The maximum angle around the vertical that can be looked through in the positive direction.  For best results, use values less than 180° to limit the view, or exactly 360° to allow full rotation.")]
        [Range(0f, 360f)]
        public float MaxPositiveAngle;
        [Tooltip("The maximum angle around the vertical that can be looked through in the negative direction.  For best results, use values greater than -180° to limit the view, or exactly -360° to allow full rotation.")]
        [Range(-360f, 0f)]
        public float MaxNegativeAngle;
        [Tooltip("If true, then the look rotation is applied using physics, otherwise it is applied using kinematic Transform rotation.")]
        public bool UsePhysicsToLook;
        [Tooltip("Around what axis will the look rotation be applied?")]
        public AxisDirection AxisDirectionType;
        [Tooltip("Only required if " + nameof(AxisDirectionType) + " is " + nameof(AxisDirection.CustomWorldSpace) + " or " + nameof(AxisDirection.CustomLocalSpace) + ".")]
        public Vector3 CustomAxisDirection;

        /// <summary>
        /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
        /// </summary>
        /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
        public Vector3 GetUpwardUnitVector() {
            switch (AxisDirectionType) {
                case AxisDirection.WithGravity: return Physics.gravity.normalized;
                case AxisDirection.OppositeGravity: return -Physics.gravity.normalized;
                case AxisDirection.CustomWorldSpace: return CustomAxisDirection.normalized;
                case AxisDirection.CustomLocalSpace: return (UsePhysicsToLook ? RigidbodyToRotate.transform : TransformToRotate).TransformDirection(CustomAxisDirection.normalized);
                default: throw new NotImplementedException(ConditionalLogger.GetSwitchDefault(AxisDirectionType));
            }
        }

        // EVENT HANDLERS
        private void Reset() {
            MaxPositiveAngle = 360f;
            MaxNegativeAngle = -360f;
            UsePhysicsToLook = true;
            CustomAxisDirection = Vector3.up;
        }
        private void Awake() {
            if (UsePhysicsToLook)
                Assert.IsNotNull(RigidbodyToRotate, this.GetAssociationAssertion(nameof(this.RigidbodyToRotate)));
            else
                Assert.IsNotNull(TransformToRotate, this.GetAssociationAssertion(nameof(this.TransformToRotate)));

            Assert.IsNotNull(LookInput, this.GetAssociationAssertion(nameof(this.LookInput)));
        }
        private void Update() {
            _deltaSinceLast += LookInput.Value();

            if (!UsePhysicsToLook) {
                doLookRotation();
                _deltaSinceLast = 0f;
            }
        }
        private void FixedUpdate() {
            if (UsePhysicsToLook) {
                doLookRotation();
                _deltaSinceLast = 0f;
            }
        }
        private void doLookRotation() {
            // Determine the upward direction
            Vector3 up = GetUpwardUnitVector();

            // Rotate the requested number of degrees around the upward axis, using the desired method
            float deltaAngle = (_deltaSinceLast > 0) ? Mathf.Min(MaxPositiveAngle - _angle, _deltaSinceLast) : Mathf.Max(MaxNegativeAngle - _angle, _deltaSinceLast);
            if (UsePhysicsToLook)
                RigidbodyToRotate.MoveRotation(RigidbodyToRotate.rotation * Quaternion.AngleAxis(deltaAngle, up));
            else
                TransformToRotate.Rotate(up, deltaAngle, Space.World);

            // Adjust the internal angle counter
            _angle += deltaAngle;
            if (Mathf.Abs(_angle) >= 360f)
                _angle = 0f;
        }

    }

}
