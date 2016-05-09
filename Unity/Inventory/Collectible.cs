using UnityEngine;

using System.Collections;

namespace Danware.Unity.Inventory {
    
    public class Collectible : MonoBehaviour {
        // API INTERFACE
        public void Drop() {
            StartCoroutine(pauseCollectibility());
        }

        // INSPECTOR FIELDS
        public GameObject Item;
        public bool IsCollectible { get; private set; } = true;
        [Tooltip("If dropped, the item will take this many seconds to become collectible again")]
        public float DropRefactoryPeriod = 1.5f;

        // HELPER FUNCTIONS
        private IEnumerator pauseCollectibility() {
            IsCollectible = false;
            yield return new WaitForSeconds(DropRefactoryPeriod);
            IsCollectible = true;
        }
    }

}
