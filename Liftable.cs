using UnityEngine;

namespace Danware.Unity {
    
    [DisallowMultipleComponent]
    public class Liftable : MonoBehaviour {

        // INSPECTOR FIELDS
        public Transform Root;
        [Tooltip("Provides a way of enabling/disable Liftables, since this component doesn't have Start/Update functions.")]
        public bool CanLift = true;
        public Vector3 LiftOffset = new Vector3(0f, 0f, 1.5f);
        public Vector3 PreferredLiftRotation = Vector3.zero;
        public bool UsePreferredRotation = false;

        // API INTERFACE
        public Lifter Lifter { get; set; }
    }

}
