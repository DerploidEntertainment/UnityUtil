using UnityEngine;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Collider))]
    public class CollectibleDetector : MonoBehaviour {
        // HIDDEN FIELDS
        private float _closestDist = Mathf.Infinity;
        private Collectible _closest;

        // INSPECTOR FIELDS
        public Transform TargetRoot;

        // EVENT HANDLERS
        private void OnTriggerStay(Collider collider) {
            // Check if this Collider is now the closest Collider
            Vector3 pos = transform.position;
            float otherDist   = (collider.ClosestPointOnBounds(pos) - pos).sqrMagnitude;
            bool closer = (otherDist < _closestDist);

            // If so, and it is a valid Collectible, then mark it as the new closest Collectible
            if (closer) {
                Collectible c = collider.GetComponent<Collectible>();
                if (c?.IsCollectible ?? false) {
                    _closestDist = otherDist;
                    _closest = c;
                    c.Collect(TargetRoot);
                }
            }
        }
    }

}
