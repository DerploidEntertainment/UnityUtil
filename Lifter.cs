using System;

using UnityEngine;

using Danware.Unity.Input;

namespace Danware.Unity {

    [DisallowMultipleComponent]
    public class Lifter : MonoBehaviour {
        // ABSTRACT DATA TYPES
        private struct LiftableWrapper {
            public LiftableWrapper(Liftable liftable, Rigidbody rigidbody, Collider collider) {
                Liftable = liftable;
                Rigidbody = rigidbody;
                Collider = collider;
            }

            public Liftable Liftable { get; }
            public Rigidbody Rigidbody { get; }
            public Collider Collider { get; }

            private static LiftableWrapper s_empty = new LiftableWrapper(null, null, null);
            public static LiftableWrapper Empty => s_empty;
        }
        public class LifterEventArgs : EventArgs {
            public LifterEventArgs(Lifter lifter) {
                Lifter = lifter;
            }
            public Lifter Lifter { get; }
        }
        public class LoadEventArgs : LifterEventArgs {
            public LoadEventArgs(Lifter lifter, Liftable liftable) : base(lifter) {
                Liftable = liftable;
            }
            public Liftable Liftable { get; }
        }
        public class ReleasedEventArgs : LoadEventArgs {
            public ReleasedEventArgs(Lifter lifter, Liftable liftable, bool dislodged, bool thrown) : 
                base (lifter, liftable)
            {
                Dislodged = dislodged;
                Thrown = thrown;
            }
            public bool Dislodged { get; }
            public bool Thrown { get; }
        }

        // HIDDEN FIELDS
        private EventHandler<LoadEventArgs> _pickupInvoker;
        private EventHandler<ReleasedEventArgs> _releaseInvoker;
        private FixedJoint _joint;
        private JointWrapper _jointWrapper;
        private LiftableWrapper _load;

        // INSPECTOR FIELDS
        [Header("Inputs")]
        public StartStopInput LiftInput;
        public StartStopInput ThrowInput;

        [Header("Options")]
        public Rigidbody LiftingRigidbody;
        public LayerMask LiftableLayerMask;
        public float Reach = 5f;
        public float MaxMass = 10f;
        public float DislodgeForce = Mathf.Infinity;
        public float DislodgeTorque = Mathf.Infinity;

        [Header("Throwing")]
        public bool CanThrow = true;
        public float ThrowForce = 10f;

        // EVENT HANDLERS
        private void Update() {
            // Get user input
            bool toggleLift = LiftInput.Started();
            bool threw = ThrowInput.Started();

            // If the player pressed Use, then pick up or drop a load
            if (toggleLift) {
                if (_load.Liftable == null && LiftingRigidbody != null)
                    pickupActions();
                else
                    releaseActions();
            }

            // If the player pressed Throw and throwing is currently applicable, then do throw actions
            if (threw && CanThrow && _load.Liftable != null)
                throwActions();
        }

        // API INTERFACE
        public Liftable Load => _load.Liftable;
        public void Pickup() {
            if (_load.Liftable == null && LiftingRigidbody != null)
                pickupActions();
        }
        public void Release() {
            if (_load.Liftable != null)
                releaseActions();
        }
        public void Throw() {
            if (CanThrow && _load.Liftable != null)
                throwActions();
        }

        public event EventHandler<LoadEventArgs> LoadPickedUp {
            add { _pickupInvoker += value; }
            remove { _pickupInvoker -= value; }
        }
        public event EventHandler<ReleasedEventArgs> LoadReleased {
            add { _releaseInvoker += value; }
            remove { _releaseInvoker -= value; }
        }

        // HELPER FUNCTIONS
        private void pickupActions() {
            // Make sure there is no current load, and an object is ahead that can be picked up
            _load = liftableAhead();
            if (_load.Liftable == null)
                return;

            Liftable liftable = _load.Liftable;
            Transform trans = _load.Rigidbody.transform;
            liftable.Lifter = this;

            // Move the load to the correct offset/orientation
            // Cant use Rigidbody.position/rotation b/c we're about to add a Joint
            trans.position = transform.TransformPoint(liftable.LiftOffset);
            if (liftable.UsePreferredRotation)
                trans.rotation = transform.rotation * Quaternion.Euler(liftable.PreferredLiftRotation);

            // Connect it to the holder via a FixedJoint
            _jointWrapper = _load.Rigidbody.gameObject.AddComponent<JointWrapper>();
            _joint = _jointWrapper.SetJoint<FixedJoint>();
            _joint.breakForce = DislodgeForce;
            _joint.breakTorque = DislodgeTorque;
            _joint.connectedBody = LiftingRigidbody;
            _jointWrapper.Broken += (object sender, EventArgs e) => jointBreakActions();

            // Raise the PickUp event
            var args = new LoadEventArgs(this, liftable);
            _pickupInvoker?.Invoke(this, args);
        }
        private void releaseActions() {
            // Break the joint and release the Load
            Liftable oldLiftable = _load.Liftable;
            destroyJoint();
            releaseLoad();

            // Raise the Released event
            var args = new ReleasedEventArgs(this, oldLiftable, false, false);
            _releaseInvoker?.Invoke(this, args);
        }
        private void throwActions() {
            // Break the joint and release the load
            Rigidbody oldRb = _load.Rigidbody;
            Liftable oldLiftable = _load.Liftable;
            destroyJoint();
            releaseLoad();

            // Apply the throw force
            oldRb.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);

            // Raise the Thrown event
            var args = new ReleasedEventArgs(this, oldLiftable, false, true);
            _releaseInvoker?.Invoke(this, args);
        }
        private void jointBreakActions() {
            // Release the load
            Liftable oldLiftable = _load.Liftable;
            releaseLoad();

            // Raise the Released event
            var args = new ReleasedEventArgs(this, oldLiftable, true, false);
            _releaseInvoker?.Invoke(this, args);
        }
        private LiftableWrapper liftableAhead() {
            Liftable liftableAhead = null;

            // Locate any valid physical object that is within range, not too heavy, and non-kinematic
            bool loadAhead = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Reach, LiftableLayerMask);
            if (loadAhead) {
                Rigidbody rb = hitInfo.collider.attachedRigidbody;
                if (rb != null && !rb.isKinematic && rb.mass <= MaxMass) {
                    Liftable liftable = rb.GetComponentInChildren<Liftable>();
                    if (liftable != null && liftable.Lifter == null)
                        liftableAhead = liftable;
                }
            }

            // Wrap these values in a struct and return them
            var lw = new LiftableWrapper(liftableAhead, hitInfo.collider?.attachedRigidbody, hitInfo.collider);
            return lw;
        }
        private void destroyJoint() {
            DestroyImmediate(_jointWrapper);
            DestroyImmediate(_joint);
        }
        private void releaseLoad() {
            _load.Liftable.Lifter = null;
            _load = LiftableWrapper.Empty;
        }

    }

}
