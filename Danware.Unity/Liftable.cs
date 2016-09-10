using UnityEngine;

namespace Danware.Unity {
    
    [RequireComponent(typeof(Collider))]
    public class Liftable : MonoBehaviour {
        // INSPECTOR FIELDS
        public Transform Root;
        public Vector3 LiftOffset = new Vector3(0f, 0f, 1.5f);
        public Vector3 PreferredLiftRotation = Vector3.zero;
        public bool UsePreferredRotation = false;

        // API INTERFACE
        public void Lift(Transform lifter) {
            // Move the load to the correct offset/orientation
            // Cant use Rigidbody.position/rotation b/c we're about to add a Joint
            Root.position = lifter.transform.TransformPoint(LiftOffset);
            if (UsePreferredRotation)
                Root.rotation = lifter.transform.rotation * Quaternion.Euler(PreferredLiftRotation);
        }
    }

}
