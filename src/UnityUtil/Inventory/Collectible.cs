using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;

namespace UnityEngine.Inventory {

    public class Collectible : MonoBehaviour
    {
        [Required]
        public GameObject? Root;

        public float Amount = 25f;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;
        public CollectEvent Detected = new();
        public CollectEvent Used = new();
        public CollectEvent Emptied = new();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() {
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
