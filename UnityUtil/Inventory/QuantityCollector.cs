using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    [RequireComponent(typeof(Collector))]
    public class QuantityCollector : MonoBehaviour {

        // INSPECTOR FIELDS
        public ManagedQuantity Quantity;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Quantity, this.GetAssociationAssertion(nameof(this.Quantity)));

            GetComponent<Collector>().Collected.AddListener(collect);
        }
        private void collect(Collector collector, Collectible collectible) {
            // If no collectible was found then just return
            QuantityCollectible qc = collectible.GetComponent<QuantityCollectible>();
            if (qc == null)
                return;

            // If one was found, then adjust its current health as necessary
            float leftover = Quantity.Increase(collectible.Amount, qc.ChangeMode);
            collectible.Collect(collector, leftover);
        }
    }

}
