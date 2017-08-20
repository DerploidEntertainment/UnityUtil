using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    public class AmmoCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public int AmmoAmount;
        public string WeaponTypeName;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

        private void Awake() {
            Assert.IsTrue(AmmoAmount >= 0, $"{nameof(AmmoCollectible)} {transform.parent.name}{name} must have a positive {nameof(this.AmmoAmount)}!");
            Assert.IsTrue(WeaponTypeName != "", $"{nameof(AmmoCollectible)} {transform.parent.name}{name} must specify a value for {nameof(this.WeaponTypeName)}!");
        }

    }

}
