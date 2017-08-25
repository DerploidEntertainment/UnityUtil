using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace Danware.Unity.Inventory {

    public class AmmoCollector : MonoBehaviour {

        private SphereCollider _sphere;

        // INSPECTOR FIELDS
        public Inventory Inventory;
        public float Radius = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Inventory, $"{nameof(AmmoCollector)} {transform.parent.name}.{name} must be associated with an {nameof(this.Inventory)}!");

            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;
        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            // If no collectible was found then just return
            AmmoCollectible ac = other.attachedRigidbody.GetComponent<AmmoCollectible>();
            if (ac == null)
                return;

            // Try to find a Weapon with a matching name in the Inventory and adjust its ammo
            bool ammoUsed = false;
            AmmoTool tool = Inventory.GetComponentsInChildren<AmmoTool>(true)
                                     .SingleOrDefault(t => t.AmmoTypeName == ac.AmmoTypeName);
            if (tool != null) {
                int leftover = tool.Load(ac.Ammo);
                ammoUsed = (leftover < ac.Ammo);
                ac.Ammo = leftover;
            }

            // Destroy the collectible's GameObject as necessary
            if (
                (ac.DestroyMode == CollectibleDestroyMode.WhenUsed && ammoUsed) ||
                (ac.DestroyMode == CollectibleDestroyMode.WhenEmptied && ac.Ammo == 0f) ||
                (ac.DestroyMode == CollectibleDestroyMode.WhenDetected))
            {
                Destroy(ac.Root);
            }
        }
    }

}
