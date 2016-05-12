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

            // If it was successfully given, then parent the contained item to the Inventory's Transform
            base.doCollect(targetRoot);
            Item.transform.parent = inv.transform;
            Item.transform.localPosition = new Vector3(0f, 0f, 0f);
            Item.transform.localRotation = Quaternion.identity;
        }
        protected override void doDrop(Transform target) {
            // Drop it as a new Collectible
            base.doDrop(target);
            Item.transform.parent = transform;
        }
    }

}
