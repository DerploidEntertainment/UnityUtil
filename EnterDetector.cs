using UnityEngine;

namespace Danware.Unity {
    
    public class EnterDetector : DetectorBase {

        private void OnCollisionEnter(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            Responder.BeginResponse(this, coll, target);
        }

        private void OnTriggerEnter(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            Responder?.BeginResponse(this, collider, target);
        }

    }

}

