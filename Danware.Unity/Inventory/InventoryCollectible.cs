using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class InventoryCollectible : Collectible {
        // INSPECTOR FIELDS
        public GameObject Item;

        // HELPER FUNCTIONS
        protected override void doCollect(Transform targetRoot) {
            // Try to give this Item to the target Inventory
            Inventory inv = targetRoot.GetComponentInChildren<Inventory>();
            bool success = (inv == null) ? false : inv.Give(this);
            if (!success)
                return;

            // If it was successfully given, then do collect actions
            base.doCollect(targetRoot);
            PhysicalObject.SetActive(false);
            Item.transform.parent = inv.transform;
            Item.transform.localPosition = new Vector3(0f, 0f, 0f);
            Item.transform.localRotation = Quaternion.identity;
        }
        protected override void doDrop(Transform target) {
            // Drop it as a new Collectible
            PhysicalObject.SetActive(true);
            base.doDrop(target);    // Don't try base actions until the PhysicalObject has been reactivated
            PhysicalObject.transform.position = target.transform.position;
            Item.transform.parent = transform;
        }
    }

}
