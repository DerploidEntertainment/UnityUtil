using UnityEngine;
using UnityEngine.Events;
using System;

namespace Danware.Unity {

    [Serializable]
    public class DetonateEvent : UnityEvent<Collider[]> { }

    public class Detonator : MonoBehaviour {

        // INSPECTOR FIELDS
        public float ExplosionRadius = 4f;
        public LayerMask AffectLayerMask;

        public CancellableUnityEvent Detonating = new CancellableUnityEvent();
        public DetonateEvent Detonated = new DetonateEvent();

        // API INTERFACE
        public void Detonate() {
            // Raise the Detonating event, allowing listeners to cancel detonation
            Detonating.Invoke();
            if (Detonating.Cancel)
                return;

            // Do an OverlapSphere into the scene on the given Affect Layer
            // Raise the Detonated event, allowing other components to select which targets to affect
            Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius, AffectLayerMask);
            Detonated.Invoke(hits);
        }

    }

}
