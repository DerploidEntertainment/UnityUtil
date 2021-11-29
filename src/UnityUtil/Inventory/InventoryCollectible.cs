using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    public class InventoryCollectible : MonoBehaviour
    {

        public GameObject Root;
        public GameObject ItemRoot;
        [Tooltip("These should be the Colliders that allow your Collectible to be collected.  When dropped, these Colliders will be enabled/disabled to create the refactory period.")]
        public Collider[] CollidersToToggle;

        private void Awake() {
            this.AssertAssociation(Root, nameof(this.Root));
            this.AssertAssociation(ItemRoot, nameof(this.ItemRoot));
        }

    }

}
