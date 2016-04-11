using UnityEngine;

namespace Danware.Unity3D.Inventory {

    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour {
        // INSPECTOR FIELDS
        public Inventory Inventory;
        public LayerMask CollectLayer;

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(Inventory != null, "CollectibleDetector {0} was not associated with an Inventory!", this.name);
        }
        private void OnTriggerEnter(Collider other) {
            // If the triggering object is not on the right layer then return
            int layer = other.gameObject.layer;
            bool isCollectible = ((CollectLayer & (1 << layer)) != 0);
            if (!isCollectible)
                return;

            // Otherwise, tell the attached Inventory to collect the Collectible, if it is enabled
            Collectible c = other.GetComponent<Collectible>();
            if (c != null && c.isActiveAndEnabled && Inventory.Items.Count < Inventory.MaxItems)
                Inventory.Collect(c);
        }
    }

}
