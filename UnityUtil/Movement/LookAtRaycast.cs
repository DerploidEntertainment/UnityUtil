using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Inventory;

namespace UnityEngine {

    public class LookAtRaycast : Updatable {

        [Tooltip("This Transform will always rotate to look at whatever the " + nameof(RaycastingTransform) + " is looking at.  This point is at most " + nameof(Range) + " units ahead, but will be closer if an object matching " + nameof(LayerMask) + " is closer.")]
        public Transform TransformToRotate;
        [Tooltip("The " + nameof(LookAtRaycast.TransformToRotate) + " will always rotate to look at whatever the " + nameof(RaycastingTransform) + " is looking at.  This point is at most " + nameof(Range) + " units ahead, but will be closer if an object matching " + nameof(LayerMask) + " is closer.")]
        public Transform RaycastingTransform;
        [Tooltip("The " + nameof(LookAtRaycast.TransformToRotate) + " will always rotate to look at whatever the " + nameof(RaycastingTransform) + " is looking at.  This point is at most " + nameof(Range) + " units ahead, but will be closer if an object matching " + nameof(LayerMask) + " is closer.  If a " + nameof(WeaponInfo) + " is provided, then its " + nameof(UnityEngine.Inventory.WeaponInfo.Range) + " will be given priority over this value.")]
        public float Range;
        [Tooltip("The " + nameof(LookAtRaycast.TransformToRotate) + " will always rotate to look at whatever the " + nameof(RaycastingTransform) + " is looking at.  This point is at most " + nameof(Range) + " units ahead, but will be closer if an object matching " + nameof(LayerMask) + " is closer.  If a " + nameof(WeaponInfo) + " is provided, then its " + nameof(UnityEngine.Inventory.WeaponInfo.AttackLayerMask) + " will be given priority over this value.")]
        public LayerMask LayerMask;
        [Tooltip("If the " + nameof(LookAtRaycast.RaycastingTransform) + " is associated with a " + nameof(UnityEngine.Inventory.Weapon) + ", then providing its " + nameof(UnityEngine.Inventory.WeaponInfo) + " here will override " + nameof(Range) + " and " + nameof(LayerMask) + ", which might be less error-prone during development.")]
        public WeaponInfo WeaponInfo;
        [Tooltip("This upward direction will be used by the " + nameof(LookAtRaycast.TransformToRotate) + " to rotate toward whatever the " + nameof(RaycastingTransform) + " is looking at.")]
        public AxisDirection UpwardDirectionType;
        [Tooltip("Only required if " + nameof(UpwardDirectionType) + " is " + nameof(AxisDirection.CustomWorldSpace) + " or " + nameof(AxisDirection.CustomLocalSpace) + ".")]
        public Vector3 CustomUpwardDirection;

        /// <summary>
        /// Returns the unit vector that this <see cref="FollowVisionModule"/> will use to rotate towards what its associated <see cref="FollowVisionModule.VisionModule"/> is looking at.
        /// </summary>
        /// <returns>The unit vector that this <see cref="FollowVisionModule"/> will use to rotate towards what its associated <see cref="FollowVisionModule.VisionModule"/> is looking at.</returns>
        public Vector3 GetUpwardUnitVector() {
            switch (UpwardDirectionType) {
                case AxisDirection.WithGravity: return Physics.gravity.normalized;
                case AxisDirection.OppositeGravity: return -Physics.gravity.normalized;
                case AxisDirection.CustomWorldSpace: return CustomUpwardDirection.normalized;
                case AxisDirection.CustomLocalSpace: return TransformToRotate.TransformDirection(CustomUpwardDirection.normalized);
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(UpwardDirectionType));
            }
        }

        // EVENT HANDLERS
        protected override void BetterAwake() {
            Assert.IsNotNull(RaycastingTransform, this.GetAssociationAssertion(nameof(this.RaycastingTransform)));

            RegisterUpdatesAutomatically = true;
            BetterUpdate = doUpdate;
        }
        private void OnDrawGizmos() {
            float range = WeaponInfo?.Range ?? Range;
            Gizmos.DrawLine(TransformToRotate.position, TransformToRotate.TransformPoint(range * Vector3.forward));
        }
        private void doUpdate() {
            // Determine the point that the raycasting transform is looking at.
            // May be a point on an actual collider up ahead, or just a point out at its max range.
            float range = WeaponInfo?.Range ?? Range;
            LayerMask layerMask = WeaponInfo?.AttackLayerMask ?? LayerMask;
            bool somethingHit = Physics.Raycast(RaycastingTransform.position, RaycastingTransform.forward, out RaycastHit hitInfo, range, layerMask);
            Vector3 targetPos = somethingHit ? hitInfo.point : RaycastingTransform.TransformPoint(range * Vector3.forward);

            // Look at that point using the specified UpwardDirectionType
            TransformToRotate.LookAt(targetPos, GetUpwardUnitVector());
        }

    }

}
