using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class InventoryCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject Item;
        [Tooltip("These should be the Colliders that allow your Collectible to be collected.  When dropped, these Colliders will be enabled/disabled to create the refactory period.")]
        public Collider[] CollidersToToggle;
        
    }

}
