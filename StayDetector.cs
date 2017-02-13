using UnityEngine;

namespace Danware.Unity {
    
    public class StayDetector : DetectorBase {

        private void OnCollisionStay(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            Responder.BeginResponse(this, coll, target);
        }

        private void OnTriggerStay(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            Responder.BeginResponse(this, collider, target);
        }

    }

}

