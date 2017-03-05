using UnityEngine;

namespace Danware.Unity.Inventory {

    public class AmmoCollectible : MonoBehaviour, ICollectible {

        // INSPECTOR FIELDS
        public int AmmoAmount;
        public string WeaponTypeName;
        public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;

    }

}
