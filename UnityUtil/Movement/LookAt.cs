using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil {

    public class LookAt : BetterBehaviour {

        public Transform TransformToRotate;
        public Transform TransformToLookAt;
        public bool FlipOnLocalY = false;

        protected override void BetterAwake() {
            Assert.IsNotNull(TransformToRotate, this.GetAssociationAssertion(nameof(this.TransformToRotate)));
            Assert.IsNotNull(TransformToLookAt, this.GetAssociationAssertion(nameof(this.TransformToLookAt)));

            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }
        private void look() {
            TransformToRotate.LookAt(TransformToLookAt, -Physics.gravity);
            if (FlipOnLocalY)
                TransformToRotate.localRotation *= Quaternion.Euler(180f * Vector3.up);
        }

    }

}
