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
            // If no AmmoCollectible was found then just return
            AmmoCollectible a = other.attachedRigidbody.GetComponent<AmmoCollectible>();
            if (a == null)
                return;

            // Try to find a Weapon with a matching name in the Inventory
            Weapon weapon = Inventory.GetComponentsInChildren<Weapon>(true)
                                     .SingleOrDefault(w => w.WeaponName == a.WeaponTypeName);
            if (weapon == null)
                return;

            // If one was found, then adjust its current ammo as necessary
            int ammo = 0;
            int currAmmo = weapon.BackupAmmo + weapon.CurrentClipAmmo;
            int maxAmmo = weapon.MaxClips * weapon.MaxClipAmmo;
            if (currAmmo != maxAmmo) {
                ammo = Mathf.Min(maxAmmo - currAmmo, a.AmmoAmount);
                weapon.Load(ammo);
                a.AmmoAmount -= ammo;
            }

            // Destroy the AmmoCollectible's GameObject as necessary
            bool detectDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenDetected);
            bool useDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenUsed && ammo > 0f);
            bool emptyDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenEmptied && a.AmmoAmount == 0f);
            if (detectDestroy || useDestroy || emptyDestroy)
                Destroy(a.gameObject);
        }
    }

}
