using UnityEngine;

namespace Danware.Unity {

    public class TargetFollower : MonoBehaviour {
        // INSPECTOR FIELDS
        [Tooltip("An array of GameObjects that will all follow the Target at the given Offset")]
        public Transform[] Followers;
        [Tooltip("The Target being followed by all Followers at the given Offset")]
        public Transform Target;
        [Tooltip("The Offset at which all Followers will follow the Target")]
        public Vector3 Offset = new Vector3(0f, 0f, -10f);

        // EVENT HANDLERS
        private void Update() {
            for (int f = 0; f < Followers.Length; ++f)
                Followers[f].transform.position = Target.position + Offset;
        }
    }

}
