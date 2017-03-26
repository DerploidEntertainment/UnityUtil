using UnityEngine;

namespace Danware.Unity {

    public class StayDetector : ColliderDetectorBase {

        private void OnCollisionStay(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(coll, target);
        }

        private void OnTriggerStay(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(collider, target);
        }

    }

}
