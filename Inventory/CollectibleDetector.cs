using UnityEngine;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour {
        // INSPECTOR FIELDS
        [Tooltip("Collectibles will only affect GameObjects parented to this root Object (usually the root of this detector).")]
        public Transform TargetRoot;

        // EVENT HANDLERS
        private void OnTriggerEnter(Collider collider) {
            if (TargetRoot != null) {
                PhysTarget pt = collider.GetComponent<PhysTarget>();
                Collectible c = pt?.TargetComponent as Collectible;
                c?.Collect(TargetRoot);
            }
        }
    }

}
