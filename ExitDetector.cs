using UnityEngine;

namespace Danware.Unity {
    
    public class ExitDetector : DetectorBase {

        private void OnCollisionExit(Collision collision) {
            if (Responder != null) {
                Collider coll = collision.collider;
                MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(coll, target);
            }
        }

        private void OnTriggerExit(Collider collider) {
            if (Responder != null) {
                MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(collider, target);
            }
        }

    }

}

