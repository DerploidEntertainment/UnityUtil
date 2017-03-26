using System;
using UnityEngine;

namespace Danware.Unity.Inventory {

    public class InventoryCollector : DetectorResponder {

        // INSPECTOR FIELDS
        public Inventory Inventory;
        public EnterDetector[] Detectors;

        // EVENT HANDLERS
        public void Awake() {
            foreach (EnterDetector detector in Detectors)
                detector.Detected += Detector_Detected;
        }
        protected override void Detector_Detected(object sender, ColliderDetectedEventArgs e) {
            // If we are not associated with an Inventory, or if the target is not an InventoryCollectible, then just early exit
            if (Inventory == null) {
                Debug.LogWarning($"{nameof(InventoryCollector)} could not collect a {nameof(InventoryCollectible)} because it was not associated with an {nameof(Inventory)}!");
                return;
            }
            var c = e.Target as InventoryCollectible;
            if (c == null)
                return;

            // Otherwise, tell the Inventory to collect the Collectible
            Inventory.Collect(c);
        }
    }

}
