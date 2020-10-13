using System.Diagnostics.CodeAnalysis;
using UnityEngine.Logging;

namespace UnityEngine
{

    public class HoverForce : MonoBehaviour {

        private RaycastHit _lastGroundHit;

        [Tooltip("The Rigidbody to which the hover force will be applied.")]
        public Rigidbody HoveringRigidbody;
        [Tooltip("The current height at which the " + nameof(HoverForce.HoveringRigidbody) + " can be kept aloft.  Note that the hover force will automatically scale down for lower hover heights.")]
        public float HoverHeight;
        [Tooltip("If the ground beneath the " + nameof(HoverForce.HoveringRigidbody) + " makes an angle to the upward direction that is steeper than this angle, then the hover force will not be applied.  This prevents the " + nameof(HoverForce.HoveringRigidbody) + " from 'climbing' steep walls.")]
        [Range(0f, 90f)]
        public float MaxAngleToSurface;
        [Tooltip("If true, then the hover force will only be applied while the " + nameof(HoverForce.HoveringRigidbody) + " is grounded (i.e., <= " + nameof(HoverForce.HoverHeight) + " units above a surface), causing it to fall at the normal rate.  If false, then the hover force will be applied even when the " + nameof(HoverForce.HoveringRigidbody) + " is not grounded, allowing it to 'float' down more gently.")]
        public bool OnlyApplyForceWhileGrounded = true;
        [Tooltip("The maximum mass of " + nameof(HoverForce.HoveringRigidbody) + " that this " + nameof(UnityEngine.HoverForce) + " can keep aloft at the " + nameof(HoverForce.HoverHeight) + ".  If set to Infinity, then a Rigidbody of any mass can be kept aloft at the same " + nameof(HoverHeight) + "; otherwise, Rigidbodies more massive than this value will sink to the ground.  Note that the hover force will automatically scale down for lower " + nameof(HoverForce.HoveringRigidbody) + " masses.")]
        public float MaxHoverableMass;
        [Tooltip("Only Colliders matching this layer mask will be repelled against by the hover force.  That is, the " + nameof(HoverForce.HoveringRigidbody) + " will 'fall through' colliders that are not in this layer mask.")]
        public LayerMask GroundLayerMask;
        [Tooltip("What axis should be considered upward?  That is, along what axis will the hover force push the " + nameof(HoverForce.HoveringRigidbody) + " to keep it aloft?")]
        public AxisDirection UpwardDirectionType;
        [Tooltip("Only required if " + nameof(UpwardDirectionType) + " is " + nameof(AxisDirection.CustomWorldSpace) + " or " + nameof(AxisDirection.CustomLocalSpace) + ".")]
        public Vector3 CustomUpwardDirection;
        [Tooltip("If true, then the hover force will be applied using the AddForceAtPosition() method, which looks more realistic when this " + nameof(UnityEngine.HoverForce) + " and the " + nameof(HoverForce.HoveringRigidbody) + " are part of the same in-game entity.  If false, then the hover force will be applied using the AddForce() method, which ignores the position of this " + nameof(UnityEngine.HoverForce) + " relative to the " + nameof(HoverForce.HoveringRigidbody) + ".")]
        public bool AddForceAtPosition = true;

        /// <summary>
        /// Returns the unit vector in which this <see cref="HoverForce"/> will attempt to hover.
        /// </summary>
        /// <returns>The unit vector in which this <see cref="HoverForce"/> will attempt to hover.</returns>
        public Vector3 GetUpwardUnitVector() =>
            UpwardDirectionType switch {
                AxisDirection.WithGravity => Physics.gravity.normalized,
                AxisDirection.OppositeGravity => -Physics.gravity.normalized,
                AxisDirection.CustomWorldSpace => CustomUpwardDirection.normalized,
                AxisDirection.CustomLocalSpace => transform.TransformDirection(CustomUpwardDirection.normalized),
                _ => throw UnityObjectExtensions.SwitchDefaultException(UpwardDirectionType),
            };
        public float AppliedFractionOfMaxForce { get; private set; }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() {
            HoverHeight = 2f;
            MaxAngleToSurface = 60f;
            MaxHoverableMass = 1f;
            UpwardDirectionType = AxisDirection.OppositeGravity;
            CustomUpwardDirection = Vector3.up;
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void FixedUpdate() {
            if (HoveringRigidbody == null)
                return;

            // Determine the upward direction
            Vector3 up = GetUpwardUnitVector();

            // If there is a surface below the hovering Rigidbody, and its angle is not too oblique,
            // then apply a hover force to the Rigidbody that scales inversely with the distance from the surface
            Vector3 down = -up;
            Vector3 pos = HoveringRigidbody.position;
            bool surfaceBelow = Physics.Raycast(pos, down, out _lastGroundHit, HoverHeight, GroundLayerMask, QueryTriggerInteraction.Ignore);
            if (surfaceBelow) {
                float angle = Vector3.Angle(up, _lastGroundHit.normal);
                if (angle <= MaxAngleToSurface)
                    applyHoverForce(up);
            }
        }

        private void applyHoverForce(Vector3 up) {
            float massToLift = Mathf.Min(HoveringRigidbody.mass, MaxHoverableMass);
            float weightToLift = massToLift * Physics.gravity.magnitude;
            AppliedFractionOfMaxForce = Mathf.Max(1f - _lastGroundHit.distance / HoverHeight, 0f);
            Vector3 pushForce = weightToLift * (1 + AppliedFractionOfMaxForce) * up;

            if (AddForceAtPosition)
                HoveringRigidbody.AddForceAtPosition(pushForce, transform.position);
            else
                HoveringRigidbody.AddForce(pushForce);
        }

    }

}
