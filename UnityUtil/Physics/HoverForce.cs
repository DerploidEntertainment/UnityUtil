using System;
using UnityEngine.Assertions;

namespace UnityEngine {

    public class HoverForce : MonoBehaviour {

        [Tooltip("The Rigidbody to which the hover force will be applied.")]
        public Rigidbody HoveringRigidbody;
        [Tooltip("The current height at which the associated Rigidbody can be kept aloft.  Note that the hover force will automatically scale down for lower hover heights.")]
        public float HoverHeight;
        [Tooltip("The maximum height at which this " + nameof(UnityEngine.HoverForce) + " will attempt to keep the associated Rigidbody aloft.  Must not be greater than " + nameof(MaxHoverHeight) + ".  Note that the hover force will automatically scale down for lower hover heights.")]
        public float MaxHoverHeight;
        [Tooltip("If the ground beneath the hovering Rigidbody makes an angle to the upward direction that is steeper than this angle, then the hover force will not be applied.  This prevents the hovering Rigidbody from 'climbing' steep walls.")]
        [Range(0f, 90f)]
        public float MaxAngleToSurface;
        [Tooltip("The maximum mass of associated Rigidbody that this " + nameof(UnityEngine.HoverForce) + " can keep aloft at the " + nameof(MaxHoverHeight) + ".  If set to Infinity, then Rigidbodies of any mass can be kept aloft at the same " + nameof(HoverHeight) + "; otherwise, Rigidbodies more massive than this value will sink to the ground.  Note that the hover force will automatically scale down for lower hover heights and Rigidbody masses.")]
        public float MaxHoverableMass;
        [Tooltip("Only colliders matching this layer mask will be repeled against by the hover force.  That is, the associated Rigidbody will 'fall through' colliders that are not in this layer mask.")]
        public LayerMask GroundLayerMask;
        [Tooltip("What axis should be considered upward?  That is, along what axis will the hover force push the associated Rigidbody to keep it aloft?")]
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
                case AxisDirection.CustomLocalSpace: return transform.TransformDirection(CustomUpwardDirection.normalized);
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(UpwardDirectionType));
            }
        }

        private void Reset() {
            HoverHeight = 2f;
            MaxHoverHeight = 5f;
            MaxAngleToSurface = 60f;
            MaxHoverableMass = 1f;
            UpwardDirectionType = AxisDirection.OppositeGravity;
            CustomUpwardDirection = Vector3.up;
        }
        private void Awake() {
            Assert.IsTrue(HoverHeight <= MaxHoverHeight, $"{this.GetHierarchyNameWithType()} cannot have a {nameof(this.HoverHeight)} higher than its {nameof(this.MaxHoverHeight)}!");
        }
        private void FixedUpdate() {
            if (HoveringRigidbody == null)
                return;

            // Determine the upward direction
            Vector3 up = GetUpwardUnitVector();

            // If there is a surface below the hovering Rigidbody, and its angle is not too oblique,
            // then apply a hover force to the Rigidbody that scales inversely with the distance from the surface
            Vector3 down = -up;
            Vector3 pos = HoveringRigidbody.position;
            bool surfaceBelow = Physics.Raycast(pos, down, out RaycastHit hitInfo, HoverHeight, GroundLayerMask);
            if (surfaceBelow) {
                float angle = Vector3.Angle(up, hitInfo.normal);
                if (angle <= MaxAngleToSurface)
                    applyHoverForce(hitInfo, up);
            }
        }

        private void applyHoverForce(RaycastHit hitInfo, Vector3 up) {
            float massToLift = Mathf.Min(HoveringRigidbody.mass, MaxHoverableMass);
            float weightToLift = massToLift * Physics.gravity.magnitude;
            float heightToReach = Mathf.Min(HoverHeight, MaxHoverHeight);
            float offsetFactor = Mathf.Max(1f - hitInfo.distance / heightToReach, 0f);
            Vector3 pushForce = weightToLift * (1 + offsetFactor ) * up;

            HoveringRigidbody.AddForce(pushForce);
        }

    }

}
