using System;
using UnityEngine.Events;

namespace UnityEngine.Inventory {

    /// <summary>
    /// Type arguments are (Collector collector, Collectible collectible)
    /// </summary>
    [Serializable]
    public class CollectEvent : UnityEvent<Collector, Collectible> { }

    public class Collector : MonoBehaviour {

        private SphereCollider _sphere;

        // INSPECTOR FIELDS
        public float Radius = 1f;
        public CollectEvent Collected = new CollectEvent();

        // EVENT HANDLERS
        protected virtual void Awake() {
            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;
        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            // If no collectible was found then just return
            Collectible c = other.attachedRigidbody.GetComponent<Collectible>();
            if (c != null)
                Collected.Invoke(this, c);
        }

    }

}
