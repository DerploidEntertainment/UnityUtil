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
            Assert.IsNotNull(Root, $"{nameof(AmmoCollectible)} {transform.parent.name}.{name} must be associated with a {nameof(this.Root)}!");
            Assert.IsTrue(Ammo >= 0, $"{nameof(AmmoCollectible)} {transform.parent.name}.{name} must have a positive amount of {nameof(this.Ammo)}!");
            Assert.IsTrue(AmmoTypeName != "", $"{nameof(AmmoCollectible)} {transform.parent.name}.{name} must specify a value for {nameof(this.AmmoTypeName)}!");
        }

    }

}
