namespace UnityEngine {

    public class LookAt : Updatable {

        [Tooltip("This Transform will be rotated to look at the " + nameof(LookAt.TransformToRotate) + " or " + nameof(LookAt.TagToLookAt) + " depending on which is provided.")]
        public Transform TransformToRotate;
        [Tooltip("The " + nameof(LookAt.TransformToRotate) + " will be rotated to look at this Transform.  This value overrides the " + nameof(LookAt.TagToLookAt) + " field.")]
        public Transform TransformToLookAt;
        [Tooltip("The " + nameof(LookAt.TransformToRotate) + " will be rotated to look at the first GameObject with this Tag.  Useful for when the object/transform to be looked at will change at runtime.")]
        public string TagToLookAt;
        public bool FlipOnLocalY = false;

        protected override void Awake() {
            base.Awake();

            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }
        private void look(float deltaTime) {
            if (TransformToRotate is null || (TransformToLookAt is null && TagToLookAt is null))
                return;

            Transform target = (TagToLookAt is null) ? TransformToLookAt : GameObject.FindWithTag(TagToLookAt)?.transform;
            if (target is not null) {
                TransformToRotate.LookAt(target, -Physics.gravity);
                if (FlipOnLocalY)
                    TransformToRotate.localRotation *= Quaternion.Euler(180f * Vector3.up);
            }
        }

    }

}
