using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    [RequireComponent(typeof(Collectible))]
    public class AmmoCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public string AmmoTypeName;

        private void Awake() =>
            Assert.IsTrue(AmmoTypeName != "", $"{this.GetHierarchyNameWithType()} must specify a value for {nameof(this.AmmoTypeName)}!");

    }

}
