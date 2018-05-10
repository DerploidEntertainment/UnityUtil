using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine {

    public class LookAt : Updatable {

        public Transform TransformToRotate;
        public Transform TransformToLookAt;
        public bool FlipOnLocalY = false;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }
        private void look() {
            if (TransformToRotate == null || TransformToLookAt == null)
                return;

            TransformToRotate.LookAt(TransformToLookAt, -Physics.gravity);
            if (FlipOnLocalY)
                TransformToRotate.localRotation *= Quaternion.Euler(180f * Vector3.up);
        }

    }

}
