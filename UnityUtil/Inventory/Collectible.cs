using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Inventory {

    public class Collectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject Root;
        public float Amount = 25f;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;
        public CollectEvent Detected = new CollectEvent();
        public CollectEvent Used = new CollectEvent();
        public CollectEvent Emptied = new CollectEvent();

        private void Awake() {
            Assert.IsNotNull(Root, this.GetAssociationAssertion(nameof(this.Root)));
            Assert.IsTrue(Amount >= 0, $"{this.GetHierarchyNameWithType()} must have a positive {nameof(this.Amount)}!");
        }

        public void Collect(Collector collector, float newValue) {
            // Set the new amount
            float change = newValue - Amount;
            Amount = newValue;

            // Raise collection events, as necessary
            Detected.Invoke(collector, this);
            if (change != 0f)
                Used.Invoke(collector, this);
            if (newValue == 0f)
                Emptied.Invoke(collector, this);

            // Destroy the Root GameObject, if necessary
            if (
                (DestroyMode == CollectibleDestroyMode.WhenUsed && change != 0f) ||
                (DestroyMode == CollectibleDestroyMode.WhenEmptied && newValue == 0f) ||
                (DestroyMode == CollectibleDestroyMode.WhenDetected)) {
                Destroy(Root);
            }
        }

    }

}
