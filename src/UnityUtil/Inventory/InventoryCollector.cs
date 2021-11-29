using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    public class InventoryCollector : MonoBehaviour {

        private SphereCollider _sphere;

        public Inventory Inventory;
        public float Radius = 1f;

        private void Awake() {
            this.AssertAssociation(Inventory, nameof(this.Inventory));

            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;
        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            InventoryCollectible c = other.attachedRigidbody.GetComponent<InventoryCollectible>();
            if (c is not null)
                Inventory.Collect(c);
        }

    }

}
