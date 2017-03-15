using UnityEngine;

namespace Danware.Unity {
    
    public class EnterDetector : DetectorBase {

        private void OnCollisionEnter(Collision collision) {
            if (Responder != null) {
                Collider coll = collision.collider;
                MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(coll, target);
            }
        }

        private void OnTriggerEnter(Collider collider) {
            if (Responder != null) {
                MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(collider, target);
            }
        }

    }

}

