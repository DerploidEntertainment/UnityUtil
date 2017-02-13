using UnityEngine;

namespace Danware.Unity {
    
    public class ExitDetector : DetectorBase {

        private void OnCollisionExit(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            Responder.BeginResponse(this, coll, target);
        }

        private void OnTriggerExit(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            Responder.BeginResponse(this, collider, target);
        }

    }

}

