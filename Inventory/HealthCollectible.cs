using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {
    
    public class HealthCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public float HealthAmount = 25f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

        private void Awake() =>
            Assert.IsTrue(HealthAmount >= 0, $"{nameof(AmmoCollectible)} {transform.parent.name}{name} must have a positive {nameof(this.HealthAmount)}!");

    }

}
