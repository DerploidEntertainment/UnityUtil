using UnityEngine;

namespace Danware.Unity {

    public class StayDetector : DetectorBase {

        private void OnCollisionStay(Collision collision) {
            if (Responder != null) {
                Collider coll = collision.collider;
                MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(coll, target);
            }
        }

        private void OnTriggerStay(Collider collider) {
            if (Responder != null) {
                MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
                onDetected(collider, target);
            }
        }

    }

}
