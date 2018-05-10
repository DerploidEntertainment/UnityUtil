using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Inventory {

    public class InventoryCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject Root;
        public GameObject ItemRoot;
        [Tooltip("These should be the Colliders that allow your Collectible to be collected.  When dropped, these Colliders will be enabled/disabled to create the refactory period.")]
        public Collider[] CollidersToToggle;

        private void Awake() {
            Assert.IsNotNull(Root, this.GetAssociationAssertion(nameof(this.Root)));
            Assert.IsNotNull(ItemRoot, this.GetAssociationAssertion(nameof(this.ItemRoot)));
        }

    }

}
