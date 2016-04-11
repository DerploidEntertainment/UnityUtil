using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using Danware.Unity3D.Input;

namespace Danware.Unity3D.Inventory {

    public class Firearm : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class FirearmEventArgs : EventArgs {
            public Firearm Firearm;
        }
        public class CancelEventArgs : EventArgs {
            public Firearm Firearm;
            public bool Cancel;
        }
        public class HitEventArgs : FirearmEventArgs {
            public RaycastHit Hit;
        }
        public class FireEventArgs : FirearmEventArgs {
            public RaycastHit[] Hits;
            public IList<RaycastHit> TargetsToAffect;
        }

        // HIDDEN FIELDS
        private EventHandler<CancelEventArgs> _firingInvoker;
        private EventHandler<FireEventArgs> _firedInvoker;
        private EventHandler<HitEventArgs> _affectingInvoker;
        private bool _canFire = true;

        // INSPECTOR FIELDS
        public float Range;
        public float FireRate = 5f; // Shots/sec
        public bool Automatic;
        public LayerMask FireLayer;
        public event EventHandler<CancelEventArgs> Firing {
            add { _firingInvoker += value; }
            remove { _firingInvoker -= value; }
        }
        public event EventHandler<FireEventArgs> Fired {
            add { _firedInvoker += value; }
            remove { _firedInvoker -= value; }
        }
        public event EventHandler<HitEventArgs> AffectingTarget {
            add { _affectingInvoker += value; }
            remove { _affectingInvoker -= value; }
        }

        // API INTERFACE
        public static StartStopInput FireInput { get; set; }
        public void Fire() {
            // If the Firearm is automatic, only allow firing in time with the FireRate
            if (Automatic && _canFire) {
                _canFire = false;
                doFire();
                Invoke("allowFire", 1f / FireRate);
            }

            // Otherwise, do the Fire
            else if (!Automatic)
                doFire();
        }

        // EVENT HANDLERS
        private void Update() {
            // Get player input
            bool fired = FireInput.Started;
            bool firing = FireInput.Happening;

            // Try to Fire according to whether the Firearm is automatic
            if (!Automatic && fired)
                Fire();
            if (Automatic && firing)
                Fire();
        }
        private void OnDrawGizmos() {
            Gizmos.DrawRay(transform.position, Range * transform.forward);
        }

        // HELPER FUNCTIONS
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

            // Raise the Fired event, allowing other components to select which targets to affect
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, Range, FireLayer);
            hits = hits.OrderBy(h => h.distance).ToArray();
            FireEventArgs fireArgs = new FireEventArgs() {
                Firearm = this,
                Hits = hits,
                TargetsToAffect = new List<RaycastHit>(),
            };
            _firedInvoker?.Invoke(this, fireArgs);

            // Affect the closest target, if there was one
            RaycastHit[] orderedHits = fireArgs.TargetsToAffect.OrderBy(t => t.distance).ToArray();
            if (orderedHits.Length > 0) {
                HitEventArgs targetArgs = new HitEventArgs() {
                    Firearm = this,
                    Hit = orderedHits[0],
                };
                _affectingInvoker?.Invoke(this, targetArgs);
            }
        }
        private void allowFire() {
            _canFire = true;
        }

    }

}
