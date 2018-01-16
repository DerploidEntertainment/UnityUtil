using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    public class HealthCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject Root;
        public float Health = 25f;
        public Health.ChangeMode HealthChangeMode = UnityUtil.Health.ChangeMode.Absolute;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

        private void Awake() {
            Assert.IsNotNull(Root, this.GetAssociationAssertion(nameof(this.Root)));
            Assert.IsTrue(Health >= 0, $"{this.GetHierarchyNameWithType()} must have a positive amount of {nameof(this.Health)}!");
        }

    }

}
