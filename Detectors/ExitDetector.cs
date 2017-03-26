using UnityEngine;

namespace Danware.Unity {
    
    public class ExitDetector : ColliderDetectorBase {

        private void OnCollisionExit(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(coll, target);
        }

        private void OnTriggerExit(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(collider, target);
        }

    }

}

