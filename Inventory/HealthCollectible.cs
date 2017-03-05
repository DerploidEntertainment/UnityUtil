using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class HealthCollectible : MonoBehaviour, ICollectible {

        // INSPECTOR FIELDS
        public float HealthAmount = 25f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

    }

}
