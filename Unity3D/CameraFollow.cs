using UnityEngine;

namespace Danware.Unity3D {

    public class CameraFollow : MonoBehaviour {
        // INSPECTOR FIELDS
        public Camera Camera;
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 0f, -10f);

        // EVENT HANDLERS
        private void Awake() {

        }
        private void Update() {
            Camera.transform.position = Target.position + Offset;
        }
    }

}
