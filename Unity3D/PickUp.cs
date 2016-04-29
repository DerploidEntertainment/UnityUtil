using System;

using UnityEngine;

using Danware.Unity3D.Input;

namespace Danware.Unity3D {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]   // For making fixed joints
    public class PickUp : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class PickUpEventArgs : EventArgs {
            public PickUp PickUp;
        }
        public class LoadEventArgs : PickUpEventArgs {
            public Rigidbody Load;
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
        private Rigidbody _rigidbody;
        private Rigidbody _load;

        // INSPECTOR FIELDS
        public LayerMask PickupLayer = Physics.DefaultRaycastLayers;
        public float MaxMass = 10f;
        public float Reach = 5f;
        public float ThrowForce = 10f;
        public bool CanThrow = true;
        public Vector3 LocalOffset = new Vector3(0f, 0f, 1.5f);
        public float DislodgeForce = Mathf.Infinity;
        public float DislodgeTorque = Mathf.Infinity;
        public event EventHandler<LoadEventArgs> LoadPickedUp {
            add { _pickupInvoker += value; }
            remove { _pickupInvoker -= value; }
        }
        public event EventHandler<ReleasedEventArgs> LoadReleased {
            add { _releaseInvoker += value; }
            remove { _releaseInvoker -= value; }
        }

        // EVENT HANDLERS
        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
        }
        private void Update() {
            // Get user input
            bool pickup = PickupInput.Started;
            bool threw = ThrowInput.Started;

            // If the player pressed Use, then pick up or drop a load
            if (pickup) {
                if (_load == null)
                    pickupActions();
                else
                    releaseActions();
            }

            // If the player pressed Throw and throwing is currently applicable, then do throw actions
            if (threw && CanThrow && _load != null)
                throwActions();
        }

        // API INTERFACE
        public static StartStopInput PickupInput { get; set; }
        public static StartStopInput ThrowInput { get; set; }
        public Rigidbody Load { get { return _load; } }
        public void Pickup() {
            if (_load == null)
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
            // Cant use Rigidbody.position/rotation b/c we're about to add a Joint
            _load.transform.position = transform.TransformPoint(LocalOffset);
            _load.transform.rotation = transform.rotation;

            // Connect it to the holder via a FixedJoint
            _jointWrapper = _load.gameObject.AddComponent<JointWrapper>();
            _joint = _jointWrapper.SetJoint<FixedJoint>();
            _joint.breakForce = DislodgeForce;
            _joint.breakTorque = DislodgeTorque;
            _joint.connectedBody = _rigidbody;
            _jointWrapper.Broken += (object sender, EventArgs e) => jointBreakActions();

            // Raise the PickUp event
            LoadEventArgs args = new LoadEventArgs() {
                PickUp = this,
                Load = _load,
            };
            _pickupInvoker?.Invoke(this, args);
        }
        private void releaseActions() {
            // Break the joint and release the Load
            Rigidbody load = _load;
            destroyJoint();
            releaseLoad();

            // Raise the Released event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                PickUp = this,
                Load = load,
                Dislodged = false,
                Thrown = false,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private void throwActions() {
            // Break the joint and release the load
            Rigidbody load = _load;
            destroyJoint();
            releaseLoad();

            // Apply the throw force
            load.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);

            // Raise the Thrown event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                PickUp = this,
                Load = load,
                Dislodged = false,
                Thrown = true,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private void jointBreakActions() {
            // Release the load
            Rigidbody load = _load;
            releaseLoad();

            // Raise the Released event
            ReleasedEventArgs args = new ReleasedEventArgs() {
                PickUp = this,
                Load = load,
                Dislodged = true,
                Thrown = false,
            };
            _releaseInvoker?.Invoke(this, args);
        }
        private Rigidbody objAhead() {
            Rigidbody rbAhead = null;

            // Locate any valid physical object (has rigidbody/collider) that is within range, not too heavy, and non-kinematic
            RaycastHit hitInfo;
            bool loadAhead = Physics.Raycast(transform.position, transform.forward, out hitInfo, Reach, PickupLayer);
            if (loadAhead) {
                Rigidbody rb = hitInfo.rigidbody;
                if (rb != null && !rb.isKinematic && rb.mass <= MaxMass && hitInfo.collider != null)
                    rbAhead = rb;
            }

            return rbAhead;
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
