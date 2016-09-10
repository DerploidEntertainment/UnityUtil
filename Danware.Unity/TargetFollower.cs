using UnityEngine;

namespace Danware.Unity {

    public class TargetFollower : MonoBehaviour {
        // INSPECTOR FIELDS
        public Transform Follower;
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 0f, -10f);

        // EVENT HANDLERS
        private void Awake() {

        }
        private void Update() {
            Follower.transform.position = Target.position + Offset;
        }
    }

}
