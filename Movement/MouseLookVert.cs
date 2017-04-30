using UnityEngine;

using Danware.Unity.Input;

namespace Danware.Unity.Movement {

    public class MouseLookVert : MonoBehaviour {
        // ENCAPSULATED FIELDS
        private float _rotY = 0f;

        // PUBLIC FIELDS
        public float maxY = 80f;
        public float minY = -60f;

        // UNITY FUNCTIONS
        public ValueInput LookInput;
        private void Update() {
            // Get inputs
            float mouseY = LookInput.Value();

            // Rotate in y-direction
            float dy = (mouseY > 0) ? Mathf.Min(maxY - _rotY, mouseY) : Mathf.Max(minY - _rotY, mouseY);
            transform.Rotate(-dy, 0f, 0f);
            _rotY += dy;
        }
    }

}
