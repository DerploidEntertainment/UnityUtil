using Danware.Unity.Input;
using Danware.Unity.Triggers;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Danware.Unity {

    /// <summary>
    /// Represents the means by which a <see cref="Liftable"/> came to be released from a <see cref="Lifter"/>.
    /// </summary>
    public enum LiftableReleaseType {
        /// <summary>
        /// The <see cref="Liftable"/> was accidentally released, e.g. by a force breaking its connecting <see cref="Joint"/>.
        /// </summary>
        Accidental,
        /// <summary>
        /// The <see cref="Liftable"/> was purposefully released via player input.
        /// </summary>
        Purposeful,
        /// <summary>
        /// The <see cref="Liftable"/> was purposefully released by throwing.
        /// </summary>
        Thrown,
    }

    [DisallowMultipleComponent]
    public class Lifter : MonoBehaviour {
        // ABSTRACT DATA TYPES
        [Serializable]
        public class LiftablePickupEvent : UnityEvent<Liftable, Lifter> { }
        public class LiftableReleaseEvent : UnityEvent<Liftable, Lifter, LiftableReleaseType> { }

        // HIDDEN FIELDS
        private Liftable _liftable;
        private Transform _oldParent;
        private bool _oldKinematic;
        private bool _oldUseGravity;

        // INSPECTOR FIELDS
        [Header("Inputs")]
        public StartStopInput LiftInput;
        public StartStopInput ThrowInput;

        [Header("Options")]
        [Tooltip("If true, Liftables will be attached to this Lifter via the LiftingJoint.  Otherwise, the Liftable will just be parented to the LiftingObject.")]
        public bool LiftUsingPhysics = false;
        [Tooltip("If LiftUsingPhysics is true, then Liftables will be attached to this Lifter via the Joint on this GameObject.  Ignored if LiftUsingPhysics is false.")]
        public JointBreakTrigger LiftingJoint;
        [Tooltip("If LiftUsingPhysics is false, then Liftables will be attached to this Transform via parenting.  Ignored if LiftUsingPhysics is true.")]
        public Transform LiftingObject;
        public LayerMask LiftableLayerMask;
        public float Reach = 4f;
        public float MaxMass = 10f;

        [Header("Throwing")]
        public bool CanThrow = true;
        public float ThrowForce = 10f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsTrue(
                (LiftUsingPhysics && LiftingJoint != null) || (!LiftUsingPhysics && LiftingObject != null),
                $"{this.GetHierarchyNameWithType()} must have a {nameof(this.LiftingJoint)} if {nameof(this.LiftUsingPhysics)} is set to true, or a {nameof(this.LiftingObject)} if {nameof(this.LiftUsingPhysics)} is set to false.");
        }
        private void Update() {
            // Get user input
            bool toggleLift = LiftInput.Started();
            bool throwing = ThrowInput.Started();
            if (toggleLift && throwing)
                return;

            // If the player pressed Use, then pick up or drop a load
            if (toggleLift) {
                if (_liftable == null)
                    pickup();
                else {
                    LiftingJoint.Joint.connectedBody = null;
                    release(LiftableReleaseType.Purposeful);
                }
            }

            // If the player pressed Throw and throwing is currently applicable, then do throw actions
            if (throwing && CanThrow && _liftable != null)
                doThrow();
        }
        private void onJointBreak(Joint joint) => release(LiftableReleaseType.Accidental);

        // API INTERFACE
        public Liftable CurrentLiftable => _liftable;
        public LiftablePickupEvent LoadPickedUp = new LiftablePickupEvent();
        public LiftableReleaseEvent LoadReleased = new LiftableReleaseEvent();

        // HELPER FUNCTIONS
        private void pickup() {
            // Check if a physical object that's not too heavy is within range
            // If not, then just return
            Rigidbody rb = null;
            bool loadAhead = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Reach, LiftableLayerMask);
            if (loadAhead) {
                rb = hitInfo.collider.attachedRigidbody;
                if (rb != null && rb.mass <= MaxMass) {
                    _liftable = rb.GetComponent<Liftable>();
                    if (!_liftable?.CanLift ?? false)
                        _liftable = null;
                }
            }
            if (_liftable == null)
                return;

            // Connect that Liftable using Physics, if requested
            // Cant use Rigidbody.position/rotation b/c we're about to add it to a Joint
            Transform loadTrans = rb.transform;
            _oldParent = loadTrans.parent;
            _oldKinematic = rb.isKinematic;
            _oldUseGravity = rb.useGravity;
            rb.useGravity = false;
            if (LiftUsingPhysics) {
                loadTrans.position = transform.TransformPoint(_liftable.LiftOffset); 
                if (_liftable.UsePreferredRotation)
                    loadTrans.rotation = transform.rotation * Quaternion.Euler(_liftable.PreferredLiftRotation);
                LiftingJoint.Joint.connectedBody = rb;
                rb.isKinematic = false;
                LiftingJoint.Broken.AddListener(onJointBreak);
            }

            // Otherwise, connect it via parenting
            else {
                loadTrans.parent = LiftingObject;
                rb.isKinematic = true;
                loadTrans.localPosition = _liftable.LiftOffset;
                if (_liftable.UsePreferredRotation)
                    loadTrans.localRotation = Quaternion.Euler(_liftable.PreferredLiftRotation);
            }

            // Raise the PickUp event
            _liftable.Lifter = this;
            LoadPickedUp.Invoke(_liftable, this);
        }
        private void release(LiftableReleaseType releaseType) {
            // Disconnect the Liftable using Physics, if requested
            Rigidbody rb = _liftable.GetComponent<Rigidbody>();
            if (LiftUsingPhysics)
                LiftingJoint.Broken.RemoveListener(onJointBreak);

            // Otherwise, disconnect it by unparenting
            else
                _liftable.transform.parent = _oldParent;

            // Either way, adjust the Liftable's Rigidbody
            rb.isKinematic = _oldKinematic;
            rb.useGravity = _oldUseGravity;

            // Either way, raise the Released event
            _liftable.Lifter = null;
            Liftable liftable = _liftable;
            _liftable = null;
            LoadReleased.Invoke(liftable, this, releaseType);
        }
        private void doThrow() {
            // Disconnect the Liftable
            Liftable liftable = _liftable;
            LiftingJoint.Joint.connectedBody = null;
            release(LiftableReleaseType.Thrown);

            // Apply the throw force
            Rigidbody rb = liftable.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
        }

    }

}
