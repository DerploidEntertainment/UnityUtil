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
            public bool Dropped;
        }

        // HIDDEN FIELDS
        private EventHandler<LoadEventArgs> _pickupInvoker;
        private EventHandler<LoadEventArgs> _releaseInvoker;
        private EventHandler<LoadEventArgs> _throwInvoker;
        private FixedJoint _joint;
        private Rigidbody _load;
        private bool _throwing;
        private bool _releasing;

        // INSPECTOR FIELDS
        public LayerMask PickupLayer = Physics.DefaultRaycastLayers;
        public float MaxMass = 10f;
        public float Reach = 5f;
        public float ThrowForce = 100f;
        public bool CanThrow = true;
        public Vector3 Offset = new Vector3(0f, 0f, 1f);
        public float DislodgeForce = Mathf.Infinity;
        public float DislodgeTorque = Mathf.Infinity;
        public event EventHandler<LoadEventArgs> LoadPickedUp {
            add { _pickupInvoker += value; }
            remove { _pickupInvoker -= value; }
        }
        public event EventHandler<LoadEventArgs> LoadReleased {
            add { _releaseInvoker += value; }
            remove { _releaseInvoker -= value; }
        }
        public event EventHandler<LoadEventArgs> LoadThrown {
            add { _throwInvoker += value; }
            remove { _throwInvoker -= value; }
        }

        // EVENT HANDLERS
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

            // If the player pressed Throw, then do throw actions
            if (threw)
                throwActions();
        }
        public void OnJointBreak(float breakForce) {
            // Throw the load, if requested
            if (_throwing)
                doThrow();

            // In any case, actually release the load
            Rigidbody load = _load;
            _load = null;

            // Raise the Released event
            bool dropped = (!_throwing && !_releasing);
            LoadEventArgs args = new LoadEventArgs() {
                PickUp = this,
                Load = load,
                Dropped = dropped,
            };
            _releaseInvoker?.Invoke(this, args);
        }

        // API INTERFACE
        public static StartStopInput PickupInput { get; set; }
        public static StartStopInput ThrowInput { get; set; }
        public Rigidbody Load { get { return _load; } }
        public void Pickup() {
            pickupActions();
        }
        public void Release() {
            releaseActions();
        }
        public void Throw() {
            throwActions();
        }

        // HELPER FUNCTIONS
        private void pickupActions() {
            // Make sure there is no current load, and an object is ahead that can be picked up
            if (_load != null)
                return;
            _load = objAhead();
            if (_load == null)
                return;

            // Move the load to the correct offset/orientation
            _load.transform.position = transform.TransformPoint(transform.localPosition + Offset);
            _load.transform.rotation = transform.rotation;

            // Connect it to the holder via a FixedJoint
            _joint = gameObject.AddComponent<FixedJoint>();
            _joint.breakForce = DislodgeForce;
            _joint.breakTorque = DislodgeTorque;
            _joint.connectedBody = _load;

            // Raise the PickUp event
            LoadEventArgs args = new LoadEventArgs() {
                PickUp = this,
                Load = _load,
            };
            _pickupInvoker?.Invoke(this, args);
        }
        private void releaseActions() {
            // Break the joint, if a load is present
            if (_load != null) {
                _releasing = true;
                breakJoint();
            }
        }
        private void throwActions() {
            // If there is no load or throwing is not enabled then just return
            if (_load == null || !CanThrow)
                return;

            // Break the joint
            _throwing = true;
            breakJoint();
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
        private void breakJoint() {
            // Apply a huge force so we're sure the joint gets broken
            _load.AddForce(transform.forward * float.MaxValue);
        }
        private void doThrow() {
            // Actually apply the throw force
            _load.AddForce(transform.forward * ThrowForce);
            _throwing = false;

            // Raise the Thrown event
            LoadEventArgs args = new LoadEventArgs() {
                PickUp = this,
                Load = _load,
            };
            _throwInvoker?.Invoke(this, args);
        }

    }

}
