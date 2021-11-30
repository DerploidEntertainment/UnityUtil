namespace UnityEngine {

    [DisallowMultipleComponent]
    public class Liftable : MonoBehaviour
    {
        public Transform? Root;
        [Tooltip("Provides a way of enabling/disable Liftables, since this component doesn't have Start/Update functions.")]
        public bool CanLift = true;
        public Vector3 LiftOffset = new(0f, 0f, 1.5f);
        public Vector3 PreferredLiftRotation = Vector3.zero;
        public bool UsePreferredRotation = false;

        public Lifter? Lifter { get; set; }
    }

}
