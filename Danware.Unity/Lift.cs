using System;

using UnityEngine;

using Danware.Unity.Input;

namespace Danware.Unity {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]   // For making fixed joints
    public class Lift : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class LiftEventArgs : EventArgs {
            public Lift Lift;
        }
        public class LoadEventArgs : LiftEventArgs {
            public Liftable Load;
        }
        public class ReleasedEventArgs : LoadEventArgs {
            public bool Dislodged;
            public bool Thrown;
        }

        // HIDDEN FIELDS
        private EventHandler<LoadEventArgs> _pickupInvoker;
        private EventHandler<ReleasedEventArgs> _releaseInvoker;
        private FixedJoint _joint;
        private JointWrapper _jointWrapper;
        private Liftable _load;

        // INSPECTOR FIELDS
        [Header("Inputs")]
        public StartStopInput LiftInput;
        public StartStopInput ThrowInput;

        [Header("Options")]
        public Rigidbody Lifter;
        public float Reach = 5f;
        public float MaxMass = 10f;
        public LayerMask LiftLayer = Physics.DefaultRaycastLayers;
        public float DislodgeForce = Mathf.Infinity;
        public float DislodgeTorque = Mathf.Infinity;

        [Header("Throwing")]
        public bool CanThrow = true;
        public float ThrowForce = 10f;

        // EVENT HANDLERS
        private void Update() {
            // Get user input
            bool pickup = LiftInput.Started;
            bool threw = ThrowInput.Started;

            // If the player pressed Use, then pick up or drop a load
            if (pickup) {
                if (_load == null && Lifter != null)
                    pickupActions();
                else
                    releaseActions();
            }

            // If the player pressed Throw and throwing is currently applicable, then do throw actions
            if (threw && CanThrow && _load != null)
                throwActions();
        }

        // API INTERFACE
        public event EventHandler<LoadEventArgs> LoadPickedUp {
            add { _pickupInvoker += value; }
            remove { _pickupInvoker -= value; }
        }
        public event EventHandler<ReleasedEventArgs> LoadReleased {
            add { _releaseInvoker += value; }
            remove { _releaseInvoker -= value; }
        }
        public Liftable Load { get { return _load; } }
        public void Pickup() {
            if (_load == null && Lifter != null)
                pickupActions();
        }
        public void Release() {
            if (_load != null)
                releaseActions();
        }
        public void Throw() {
            if (CanThrow && _load != null)
                throwActions();
        }

        // HELPER FUNCTIONS
        private void pickupActions() {
            // Make sure there is no current load, and an object is ahead that can be picked up
            _load = objAhead();
            if (_load == null)
                return;

            // Move the load to the correct offset/orientation
            _load.Lift(this.transform);

            // Connect it to the holder via a FixedJoint
            Rigidbody loadRb = _load.GetComponent<Collider>().attachedRigidbody;
            _jointWrapper = loadRb.gameObject.AddComponent<JointWrapper>();
            _joint = _jointWrapper.SetJoint<FixedJoint>();
            _joint.breakForce = DislodgeForce;
            _joint.breakTorque = DislodgeTorque;
            _joint.connectedBody = Lifter;
            _jointWrapper.Broken += (object sender, EventArgs e) => jointBreakActions();

            // Raise the PickUp event
            LoadEventArgs args = new LoadEventArgs() {
                Lift = this,
                Load = _load,
            };
            _pickupInvoker?.Invoke(this, args);
        }
        private void releaseActions() {
            // Break the joint and release the Load
            Liftable load = _load;
            destroyJoint();
            releaseLoad();

            // Raise the Released event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                Lift = this,
                Load = load,
                Dislodged = false,
                Thrown = false,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private void throwActions() {
            // Break the joint and release the load
            Liftable load = _load;
            destroyJoint();
            releaseLoad();

            // Apply the throw force
            Rigidbody rb = load.GetComponent<Collider>().attachedRigidbody;
            rb?.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);

            // Raise the Thrown event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                Lift = this,
                Load = load,
                Dislodged = false,
                Thrown = true,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private void jointBreakActions() {
            // Release the load
            Liftable load = _load;
            releaseLoad();

            // Raise the Released event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                Lift = this,
                Load = load,
                Dislodged = true,
                Thrown = false,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private Liftable objAhead() {
            Liftable liftAhead = null;

            // Locate any valid physical object that is within range, not too heavy, and non-kinematic
            RaycastHit hitInfo;
            bool loadAhead = Physics.Raycast(transform.position, transform.forward, out hitInfo, Reach, LiftLayer);
            if (loadAhead) {
                Liftable lift = hitInfo.collider.GetComponent<Liftable>();
                if (lift != null) {
                    Rigidbody rb = hitInfo.collider.attachedRigidbody;
                    if (!rb.isKinematic && rb.mass <= MaxMass)
                        liftAhead = lift;
                }
            }

            return liftAhead;
        }
        private void destroyJoint() {
            DestroyImmediate(_jointWrapper);
            DestroyImmediate(_joint);
        }
        private void releaseLoad() {
            _load = null;
        }

    }

}
