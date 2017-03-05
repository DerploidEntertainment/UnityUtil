using UnityEngine;

namespace Danware.Unity.Inventory {

    public class InventoryCollector : DetectorResponder {

        // INSPECTOR FIELDS
        public Inventory Inventory;

        public override void BeginResponse(DetectorBase detector, Collider targetCollider, MonoBehaviour target) {
            // If we are not associated with an Inventory, or if the target is not an InventoryCollectible, then just early exit
            if (Inventory == null) {
                Debug.LogWarning($"{nameof(InventoryCollector)} could not collect a {nameof(InventoryCollectible)} because it was not associated with an {nameof(Inventory)}!");
                return;
            }
            var c = target as InventoryCollectible;
            if (c == null)
                return;

            // Otherwise, tell the Inventory to collect the Collectible
            Inventory.Collect(c);

            // Raise the Responding event
            var args = new DetectorResponseEventArgs(this, detector, targetCollider, c);
            _respondingInvoker?.Invoke(this, args);
        }
    }

}
