using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity {

    public class LookAt : MonoBehaviour {

        public Transform TransformToRotate;
        public Transform TransformToLookAt;
        public bool FlipOnLocalY = false;

        private void Awake() {
            Assert.IsNotNull(TransformToRotate, this.GetAssociationAssertion(nameof(this.TransformToRotate)));
            Assert.IsNotNull(TransformToLookAt, this.GetAssociationAssertion(nameof(this.TransformToLookAt)));
        }
        private void Update() {
            TransformToRotate.LookAt(TransformToLookAt, -Physics.gravity);
            if (FlipOnLocalY)
                TransformToRotate.localRotation *= Quaternion.Euler(180f * Vector3.up);
        }

    }

}
