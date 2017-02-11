using UnityEngine;

namespace Danware.Unity {
    
    public class StayDetector : DetectorBase {
        
        private void OnCollisionStay(Collision collision) {
            if (_collider != null && !_collider.isTrigger)
                Responder.InitiateResponse(collision.collider, this);
        }
        private void OnTriggerStay(Collider collider) {
            if (_collider != null && _collider.isTrigger)
                Responder.InitiateResponse(collider, this);
        }

    }

}

