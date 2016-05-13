using UnityEngine;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour {
        // INSPECTOR FIELDS
        public Transform TargetRoot;

        // EVENT HANDLERS
        private void OnTriggerEnter(Collider collider) {
            Collectible c = collider.GetComponent<Collectible>();
            c.Collect(TargetRoot);
        }
    }

}
