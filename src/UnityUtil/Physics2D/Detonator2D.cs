using System;
using UnityEngine.Events;

namespace UnityEngine
{

    [Serializable]
    public class DetonateEvent2D : UnityEvent<Collider2D[]> { }

    public class Detonator2D : MonoBehaviour
    {
        public float ExplosionRadius = 4f;
        public LayerMask AffectLayerMask;

        public CancellableUnityEvent Detonating = new();
        public DetonateEvent2D Detonated = new();

        public void Detonate() {
            // Raise the Detonating event, allowing listeners to cancel detonation
            Detonating.Invoke();
            if (Detonating.Cancel)
                return;

            // Do an OverlapSphere into the scene on the given Affect Layer
            // Raise the Detonated event, allowing other components to select which targets to affect
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ExplosionRadius, AffectLayerMask);
            Detonated.Invoke(hits);
        }

    }

}
