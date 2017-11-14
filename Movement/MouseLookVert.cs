using UnityEngine;

using Danware.Unity.Input;
using UnityEngine.Assertions;

namespace Danware.Unity.Movement {

    public class MouseLookVert : MonoBehaviour {
        // HIDDEN FIELDS
        private float _rotY = 0f;

        // INSPECTOR FIELDS
        public Transform TransformToRotate;
        public ValueInput LookInput;
        public float MaxY = 80f;
        public float MinY = -60f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(TransformToRotate, $"{GetType().Name} {transform.parent?.name}.{name} was not associated with a {nameof(this.TransformToRotate)}!");

            Assert.IsNotNull(LookInput, $"{GetType().Name} {transform.parent?.name}.{name} was not associated with a {nameof(this.LookInput)}!");
        }
        private void Update() {
            // Get inputs
            float mouseY = LookInput.Value();

            // Rotate in y-direction
            float dy = (mouseY > 0) ? Mathf.Min(MaxY - _rotY, mouseY) : Mathf.Max(MinY - _rotY, mouseY);
            TransformToRotate.Rotate(-dy, 0f, 0f);
            _rotY += dy;
        }
    }

}
