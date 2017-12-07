using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    public class InventoryCollector : MonoBehaviour {

        private SphereCollider _sphere;

        // INSPECTOR FIELDS
        public Inventory Inventory;
        public float Radius = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Inventory, this.GetAssociationAssertion(nameof(this.Inventory)));

            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;
        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            InventoryCollectible c = other.attachedRigidbody.GetComponent<InventoryCollectible>();
            if (c != null)
                Inventory.Collect(c);
        }

    }

}
