using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity {

    public class HoverForce : MonoBehaviour {

        public Rigidbody HoveringRigidbody;
        public float HoverHeight = 2f;
        [Tooltip("The angle between verticalIf the ground beneath the hovering")]
        public float MaxAngleToSurface = 60f;
        public float MaxHoverForce = 10f;
        public LayerMask GroundLayerMask;
        public UpwardDirectionType UpwardDirectionType = UpwardDirectionType.OppositeGravity;
        public Vector3 CustomUpwardDirection = Vector3.up;
        public bool AutoRepelGravity = true;

        /// <summary>
        /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
        /// </summary>
        /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
        public Vector3 GetUpwardUnitVector() {
            Vector3 up = Vector3.zero;
            switch (UpwardDirectionType) {
                case UpwardDirectionType.OppositeGravity: return -Physics.gravity.normalized;
                case UpwardDirectionType.TransformUp: return transform.up;
                case UpwardDirectionType.Custom: return CustomUpwardDirection.normalized;
                default: throw new NotImplementedException($"Gah!  We haven't accounted for {nameof(Danware.Unity.UpwardDirectionType)} {UpwardDirectionType}!");
            }
        }

        private void Awake() {
            Assert.IsNotNull(HoveringRigidbody, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(HoveringRigidbody)}");
        }
        private void FixedUpdate() {
            // Determine the upward direction
            Vector3 up = GetUpwardUnitVector();

            // If there is a surface below the hovering Rigidbody, and its angle is not too oblique,
            // then apply a hover force to the Rigidbody that scales inversely with the distance from the surface
            Vector3 down = -up;
            Vector3 pos = transform.position;
            bool surfaceBelow = Physics.Raycast(pos, down, out RaycastHit hitInfo, HoverHeight, GroundLayerMask);
            if (surfaceBelow) {
                float angle = Vector3.Angle(up, hitInfo.normal);
                if (angle <= MaxAngleToSurface)
                    applyHoverForce(hitInfo, up);
            }
        }

        private void applyHoverForce(RaycastHit hitInfo, Vector3 up) {
            float pushMag = Mathf.Max(MaxHoverForce * (1f - hitInfo.distance / HoverHeight), 0f);
            Vector3 pushForce = pushMag * up;

            // Repel gravity, if requested
            if (AutoRepelGravity)
                pushForce -= Physics.gravity;

            HoveringRigidbody.AddForce(pushForce, ForceMode.Acceleration);
        }

    }

}
