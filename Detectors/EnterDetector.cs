using UnityEngine;

namespace Danware.Unity {
    
    public class EnterDetector : ColliderDetectorBase {

        private void OnCollisionEnter(Collision collision) {
            Collider coll = collision.collider;
            MonoBehaviour target = coll.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(coll, target);
        }

        private void OnTriggerEnter(Collider collider) {
            MonoBehaviour target = collider.GetComponent<PhysTarget>()?.TargetComponent;
            onDetected(collider, target);
        }

    }

}

