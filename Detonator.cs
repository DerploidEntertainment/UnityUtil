using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

namespace Danware.Unity {

    public class Detonator : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class DetonatorEventArgs : EventArgs {
            public DetonatorEventArgs(Detonator detonator) {
                Detonator = detonator;
            }
            public Detonator Detonator { get; }
        }
        public class CancelEventArgs : DetonatorEventArgs {
            public CancelEventArgs(Detonator detonator) : base(detonator) { }
            public bool Cancel { get; set; } = false;
        }
        public class DetonateEventArgs : DetonatorEventArgs {
            public DetonateEventArgs(Detonator detonator, Collider[] hits) : base(detonator) {
                Hits = hits;
            }
            public Collider[] Hits { get; }
            public Action<Collider[]> Callback { get; set; } = (collider) => { };
        }

        // HIDDEN FIELDS
        private EventHandler<CancelEventArgs> _detonatingInvoker;
        private EventHandler<DetonateEventArgs> _detonatedInvoker;
        private int _lastDetonationFrame;

        // INSPECTOR FIELDS
        public Transform EffectsPrefab;
        public float ExplosionRadius = 4f;
        public LayerMask AffectLayer;

        public event EventHandler<CancelEventArgs> Detonating {
            add { _detonatingInvoker += value; }
            remove { _detonatingInvoker -= value; }
        }
        public event EventHandler<DetonateEventArgs> Detonated {
            add { _detonatedInvoker += value; }
            remove { _detonatedInvoker -= value; }
        }

        // API INTERFACE
        public void Detonate() => doDetonate();

        // HELPER FUNCTIONS
        private void doDetonate() {
            // If a detonation has already been ordered this frame then just return
            int frame = Time.frameCount;
            if (frame == _lastDetonationFrame)
                return;
            _lastDetonationFrame = frame;

            // Raise the Detonating event, allowing listeners to cancel detonation
            var detonatingArgs = new CancelEventArgs(this);
            _detonatingInvoker?.Invoke(this, detonatingArgs);
            if (detonatingArgs.Cancel)
                return;

            // Do an OverlapSphere into the scene on the given Affect Layer
            // Raise the Detonated event, allowing other components to select which targets to affect
            Vector3 thisPos = transform.position;
            IEnumerable<Collider> hits = Physics.OverlapSphere(thisPos, ExplosionRadius, AffectLayer);
            var detonateArgs = new DetonateEventArgs(this, hits.ToArray());
            _detonatedInvoker?.Invoke(this, detonateArgs);

            // Instantiate the explosion prefab if one was provided
            if (EffectsPrefab != null)
                Instantiate(EffectsPrefab, thisPos, Quaternion.identity);

            // Affect the targets, if there were any
            detonateArgs.Callback.Invoke(detonateArgs.Hits);
        }
    }

}
