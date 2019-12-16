using UnityEngine.Assertions;
using UnityEngine.Logging;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Collectible))]
    public class AmmoCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public string AmmoTypeName;

        private void Awake() =>
            Assert.IsFalse(string.IsNullOrEmpty(AmmoTypeName), $"{this.GetHierarchyNameWithType()} must specify a value for {nameof(this.AmmoTypeName)}!");

    }

}
