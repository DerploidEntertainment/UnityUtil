using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    public class AmmoCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject Root;
        public int Ammo;
        public string AmmoTypeName;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

        private void Awake() {
            Assert.IsNotNull(Root, this.GetAssociationAssertion(nameof(this.Root)));
            Assert.IsTrue(Ammo >= 0, $"{this.GetHierarchyNameWithType()} must have a positive amount of {nameof(this.Ammo)}!");
            Assert.IsTrue(AmmoTypeName != "", $"{this.GetHierarchyNameWithType()} must specify a value for {nameof(this.AmmoTypeName)}!");
        }

    }

}
