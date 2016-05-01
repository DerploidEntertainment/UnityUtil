using UnityEngine;

namespace Danware.Unity3D.Inventory {

    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour {
        // HIDDEN FIELDS
        private float _closestDist = Mathf.Infinity;
        private Collectible _closest;

        // INSPECTOR FIELDS
        public Inventory Inventory;

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(Inventory != null, "CollectibleDetector {0} was not associated with an Inventory!", this.name);
        }
        private void OnTriggerStay(Collider other) {
            // Check if this Collider is now the closest Collider
            Vector3 pos = transform.position;
            float otherDist   = (other.ClosestPointOnBounds(pos) - pos).sqrMagnitude;
            bool closer = (otherDist < _closestDist);

            // If so, and it is a valid Collectible, then mark it as the new closest Collectible
            if (closer) {
                Collectible c = other.GetComponent<Collectible>();
                if (c?.IsCollectible ?? false) {
                    _closestDist = otherDist;
                    _closest = c;
                    doCollect();
                }
            }
        }

        // HELPER FUNCTIONS
        private void doCollect() {
            // Give the Collectible to the attached Inventory, if possible
            bool success = Inventory.Give(_closest);
            if (success)
                _closestDist = Mathf.Infinity;
        }
    }

}
