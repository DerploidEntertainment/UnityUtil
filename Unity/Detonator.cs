using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

namespace Danware.Unity {

    public class Detonator : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class DetonatorEventArgs : EventArgs {
            public Detonator Detonator;
        }
        public class CancelEventArgs : DetonatorEventArgs {
            public bool Cancel;
        }
        public class DetonateEventArgs : DetonatorEventArgs {
            public Collider[] Hits;
            public Action<Collider[]> Callback;
        }

        // HIDDEN FIELDS
        private EventHandler<CancelEventArgs> _detonatingInvoker;
        private EventHandler<DetonateEventArgs> _detonatedInvoker;

        // INSPECTOR FIELDS
        public Transform EffectsPrefab;
        public float ExplosionRadius = 4f;
        public LayerMask AffectLayer;
        public bool DestroyOnDetonate = true;

        public event EventHandler<CancelEventArgs> Detonating {
            add { _detonatingInvoker += value; }
            remove { _detonatingInvoker -= value; }
        }
        public event EventHandler<DetonateEventArgs> Detonated {
            add { _detonatedInvoker += value; }
            remove { _detonatedInvoker -= value; }
        }

        // API INTERFACE
        public void Detonate() {
            doDetonate();
        }

        // HELPER FUNCTIONS
        private void doDetonate() {
            // Raise the Detonating event, allowing listeners to cancel detonation
            CancelEventArgs detonatingArgs = new CancelEventArgs() {
                Detonator = this,
                Cancel = false,
            };
            _detonatingInvoker?.Invoke(this, detonatingArgs);
            if (detonatingArgs.Cancel)
                return;

            // Do an OverlapSphere into the scene on the given Affect Layer
            // Raise the Detonated event, allowing other components to select which targets to affect
            Vector3 thisPos = transform.position;
            IEnumerable<Collider> hits = Physics.OverlapSphere(thisPos, ExplosionRadius, AffectLayer);
            DetonateEventArgs detonateArgs = new DetonateEventArgs() {
                Detonator = this,
                Hits = hits.ToArray(),
                Callback = (target) => { },
            };
            _detonatedInvoker?.Invoke(this, detonateArgs);

            // Instantiate the explosion prefab if one was provided
            if (EffectsPrefab != null)
                Instantiate(EffectsPrefab, thisPos, Quaternion.identity);

            // Affect the targets, if there were any
            detonateArgs.Callback.Invoke(detonateArgs.Hits);

            // Destroy this object, if requested
            if (DestroyOnDetonate)
                Destroy(this.gameObject);
        }
    }

}
