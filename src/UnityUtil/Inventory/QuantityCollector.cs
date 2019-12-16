using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Collector))]
    public class QuantityCollector : MonoBehaviour {

        public ManagedQuantity Quantity;

        private void Awake() {
            this.AssertAssociation(Quantity, nameof(this.Quantity));

            GetComponent<Collector>().Collected.AddListener(collect);
        }
        private void collect(Collector collector, Collectible collectible) {
            // If no Quantity Collectible was found then just return
            QuantityCollectible qc = collectible.GetComponent<QuantityCollectible>();
            if (qc == null)
                return;

            // If one was found, then adjust its current health as necessary
            float leftover = Quantity.Increase(collectible.Amount, qc.ChangeMode);
            collectible.Collect(collector, leftover);
        }
    }

}
