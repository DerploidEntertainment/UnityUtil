using UnityEngine;

namespace Danware.Unity3D {
    
    [RequireComponent(typeof(Collider))]
    public class ColliderDetonator : Detonator {
        // EVENT HANDLERS
        public void OnCollisionEnter(Collision collision) {
            Detonate();
        }
    }

}
