using UnityEngine;

namespace Danware.Unity {
    
    public class ExitDetector : DetectorBase {
        
        private void OnCollisionExit(Collision collision) {
            if (_collider != null && !_collider.isTrigger)
                Responder.InitiateResponse(collision.collider, this);
        }
        
        private void OnTriggerExit(Collider collider) {
            if (_collider != null && _collider.isTrigger)
                Responder.InitiateResponse(collider, this);
        }
    }

}

