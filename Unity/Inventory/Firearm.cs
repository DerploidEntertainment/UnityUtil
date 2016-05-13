using UnityEngine;
using U = UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {

    public class Firearm : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public struct TargetData {
            public TargetData(uint priority = 0) {
                Priority = priority;
                Callback = (hit) => { };
            }
            public uint Priority;
            public Action<RaycastHit> Callback;
        }
        public class FirearmEventArgs : EventArgs {
            public Firearm Firearm;
        }
        public class CancelEventArgs : EventArgs {
            public Firearm Firearm;
            public bool Cancel;
        }
        public class FireEventArgs : FirearmEventArgs {
            public Vector3 Direction;
            public RaycastHit[] Hits;
            public Dictionary<RaycastHit, TargetData> TargetPriorities;
            public void Add(RaycastHit hitInfo, TargetData data) {
                // If this hit has already been added, adjust its associated data
                if (TargetPriorities.ContainsKey(hitInfo)) {
                    TargetData td = TargetPriorities[hitInfo];
                    td.Priority += data.Priority;
                    td.Callback += data.Callback;
                    TargetPriorities[hitInfo] = td;
                }

                // Otherwise, add the new association
                else
                    TargetPriorities.Add(hitInfo, data);
            }
        }

        // HIDDEN FIELDS
        private EventHandler<CancelEventArgs> _firingInvoker;
        private EventHandler<FireEventArgs> _firedInvoker;
        private bool _canStartFiring = true;
        private bool _canKeepFiring = true;
        private float _accuracyDegrees;
        private float _accuracyLerpT;

        // INSPECTOR FIELDS
        [Tooltip("A case-insensitive string to identify different types of Firearms (e.g., for collecting ammo)")]
        public string TypeID = "Firearm";
        public float Range;
        [Tooltip("Once an automatic Firearm starts firing, it maintains this many shots per second.")]
        public float FireRate = 5f;
        [Tooltip("This much time (in seconds) must pass before a Firearms can start firing again.")]
        public float RefactoryPeriod = 0f;
        public bool Automatic;
        public LayerMask AffectLayer = Physics.DefaultRaycastLayers;
        [Header("Accuracy")]
        [Range(0f, 90f)]
        public float InitialConeHalfAngle = 0f;
        [Range(0f, 90f)]
        public float FinalConeHalfAngle = 0f;
        [Tooltip("For automatic weapons, the accuracy cone's half angle will lerp from the initial to the final value in this number of seconds")]
        public float AccuracyLerpTime = 1f;   // Seconds


        // API INTERFACE
        public event EventHandler<CancelEventArgs> Firing {
            add { _firingInvoker += value; }
            remove { _firingInvoker -= value; }
        }
        public event EventHandler<FireEventArgs> Fired {
            add { _firedInvoker += value; }
            remove { _firedInvoker -= value; }
        }
        public static StartStopInput FireInput { get; set; }
        public void Fire() {
            fireStartedActions();
        }

        // EVENT HANDLERS
        private void Update() {
            // Handle player input
            if (FireInput.Started)
                fireStartedActions();

            if (FireInput.Happening)
                fireHappeningActions();

            if (FireInput.Stopped)
                fireStoppededActions();
        }
        private void OnDrawGizmos() {
            Gizmos.DrawRay(transform.position, Range * transform.forward);
        }

        // HELPER FUNCTIONS
        private void fireStartedActions() {
            // Reset the accuracy cone on the first shot
            resetAccuracy();

            // Fire, if non-automatic
            if (!Automatic && _canStartFiring)
                doFire();
            _canStartFiring = false;
        }
        private void fireHappeningActions() {
            // If the Firearm is automatic, only allow firing in time with the FireRate
            if (Automatic && _canKeepFiring) {
                _canKeepFiring = false;
                doFire();
                Invoke(nameof(allowKeepFiring), 1f / FireRate);
            }
        }
        private void fireStoppededActions() {
            bool alreadyWaiting = IsInvoking(nameof(allowStartFiring));
            if (!alreadyWaiting)
                Invoke(nameof(allowStartFiring), RefactoryPeriod);
        }
        private void resetAccuracy() {
            // Prevent player from starting firing again until after a refactory period
            _accuracyDegrees = InitialConeHalfAngle;
            _accuracyLerpT = 0f;
        }
        private void doFire() {
            // Raise the Firing event
            CancelEventArgs firingArgs = new CancelEventArgs() {
                Firearm = this,
                Cancel = false,
            };
            _firingInvoker?.Invoke(this, firingArgs);

            // If any listeners tried to cancel firing, then just return
            if (firingArgs.Cancel)
                return;

            // Get a random Ray within the accuracy cone
            float z = U.Random.Range(Mathf.Cos(Mathf.Deg2Rad * _accuracyDegrees), 1f);
            float theta = U.Random.Range(0f, 2 * Mathf.PI);
            float sqrtPart = Mathf.Sqrt(1 - z * z);
            Vector3 dir = new Vector3(sqrtPart * Mathf.Cos(theta), sqrtPart * Mathf.Sin(theta), z);
            Ray ray = new Ray(transform.position, transform.TransformDirection(dir));

            // Raycast into the scene on the given Fire Layer
            // Raise the Fired event, allowing other components to select which targets to affect
            IEnumerable<RaycastHit> hits = Physics.RaycastAll(ray, Range, AffectLayer);
            hits = hits.OrderBy(h => h.distance);
            FireEventArgs fireArgs = new FireEventArgs() {
                Firearm = this,
                Direction = ray.direction,
                Hits = hits.ToArray(),
                TargetPriorities = new Dictionary<RaycastHit, TargetData>(),
            };
            _firedInvoker?.Invoke(this, fireArgs);

            // Adjust the accuracy cone for the next shot
            _accuracyLerpT = (AccuracyLerpTime == 0 ? 1f : Mathf.Clamp01(_accuracyLerpT + (1f / FireRate) / AccuracyLerpTime));
            _accuracyDegrees = Mathf.LerpAngle(InitialConeHalfAngle, FinalConeHalfAngle, _accuracyLerpT);

            // Affect the closest, highest-priority target, if there is one
            RaycastHit[] orderedHits = (
                from derp in fireArgs.TargetPriorities
                orderby derp.Value.Priority descending,
                        derp.Key.distance
                select derp.Key).ToArray();
            if (orderedHits.Length > 0) {
                RaycastHit closest = orderedHits[0];
                TargetData td = fireArgs.TargetPriorities[closest];
                td.Callback.Invoke(closest);
            }
        }
        private void allowStartFiring() {
            _canStartFiring = true;
        }
        private void allowKeepFiring() {
            _canKeepFiring = true;
        }

    }

}
