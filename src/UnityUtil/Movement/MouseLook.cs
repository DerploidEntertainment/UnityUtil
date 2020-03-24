using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Inputs;
using UnityEngine.Logging;

namespace UnityEngine.Movement {

    public class MouseLook : Updatable {
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
        public Vector3 GetUpwardUnitVector() =>
            AxisDirectionType switch {
                AxisDirection.WithGravity => Physics.gravity.normalized,
                AxisDirection.OppositeGravity => -Physics.gravity.normalized,
                AxisDirection.CustomWorldSpace => CustomAxisDirection.normalized,
                AxisDirection.CustomLocalSpace => (UsePhysicsToLook ? RigidbodyToRotate.transform : TransformToRotate).TransformDirection(CustomAxisDirection.normalized),
                _ => throw new NotImplementedException(UnityObjectExtensions.GetSwitchDefault(AxisDirectionType)),
            };

        // EVENT HANDLERS
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Reset() {
            MaxPositiveAngle = 360f;
            MaxNegativeAngle = -360f;
            UsePhysicsToLook = true;
            CustomAxisDirection = Vector3.up;
        }
        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(LookInput, nameof(this.LookInput));

            RegisterUpdatesAutomatically = true;
            BetterUpdate = doUpdate;
            BetterFixedUpdate = doFixedUpdate;
        }
        private void doUpdate(float deltaTime) {
            _deltaSinceLast += LookInput.Value();

            if (!UsePhysicsToLook && TransformToRotate != null) {
                doLookRotation();
                _deltaSinceLast = 0f;
            }
        }
        private void doFixedUpdate(float fixedDeltaTime) {
            if (UsePhysicsToLook && RigidbodyToRotate != null) {
                doLookRotation();
                _deltaSinceLast = 0f;
            }
        }

        // HELPERS
        private void doLookRotation()
        {
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
