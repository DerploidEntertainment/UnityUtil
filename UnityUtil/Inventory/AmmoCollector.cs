using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    [RequireComponent(typeof(Collector))]
    public class AmmoCollector : MonoBehaviour {

        // INSPECTOR FIELDS
        public Inventory Inventory;
        public float Radius = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Inventory, this.GetAssociationAssertion(nameof(this.Inventory)));

            GetComponent<Collector>().Collected.AddListener(collect);
        }
        private void collect(Collector collector, Collectible collectible) {
            // If no collectible was found then just return
            AmmoCollectible ac = collectible.GetComponent<AmmoCollectible>();
            if (ac == null)
                return;

            // Try to find a Weapon with a matching name in the Inventory and adjust its ammo
            AmmoTool tool = Inventory.GetComponentsInChildren<AmmoTool>(true)
                                     .SingleOrDefault(t => t.Info.AmmoTypeName == ac.AmmoTypeName);
            if (tool != null) {
                int leftover = tool.Load((int)collectible.Amount);
                collectible.Collect(collector, leftover);
            }
        }
    }

}
