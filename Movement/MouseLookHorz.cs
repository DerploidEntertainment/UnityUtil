using UnityEngine;

using Danware.Unity.Input;
using UnityEngine.Assertions;

namespace Danware.Unity.Movement {

    public class MouseLookHorz : MonoBehaviour {
        // HIDDEN FIELDS
        private float _rotX = 0f;

        // INSPECTOR FIELDS
        public Transform TransformToRotate;
        public ValueInput LookInput;
        public float MaxX = 360f;   // For best results, minX and maxX should be either less than 180, or 360 for full rotation
        public float MinX = -360f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(TransformToRotate, $"{GetType().Name} {transform.parent?.name}.{name} was not associated with a {nameof(this.TransformToRotate)}!");

            Assert.IsNotNull(LookInput, $"{GetType().Name} {transform.parent?.name}.{name} was not associated with a {nameof(this.LookInput)}!");
        }
        private void Update() {
            // Get inputs
            float mouseX = LookInput.Value();

            // Rotate in x-direction
            float dx = (mouseX > 0) ? Mathf.Min(MaxX - _rotX, mouseX) : Mathf.Max(MinX - _rotX, mouseX);
            TransformToRotate.Rotate(0f, dx, 0f, Space.World);
            _rotX += dx;
            if (Mathf.Abs(_rotX) >= 360f)
                _rotX = 0f;
        }
    }

}
