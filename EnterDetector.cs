using UnityEngine;

namespace Danware.Unity {
    
    public class EnterDetector : DetectorBase {

        private void OnCollisionEnter(Collision collision) {
            if (_collider != null && !_collider.isTrigger)
                Responder.InitiateResponse(collision.collider, this);
        }

        private void OnTriggerEnter(Collider collider) {
            if (_collider != null && _collider.isTrigger)
                Responder.InitiateResponse(collider, this);
        }

    }

}

