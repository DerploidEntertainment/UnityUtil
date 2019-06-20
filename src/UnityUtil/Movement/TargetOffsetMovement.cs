using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Movement {

    public class TargetOffsetMovement : MonoBehaviour {

        // INSPECTOR FIELDS
        [Tooltip("The Transform to keep at the given " + nameof(Offset) + " from the " + nameof(Target))]
        public Transform TransformToMove;
        [Tooltip("The Transform being followed at the given " + nameof(Offset))]
        public Transform Target;
        [Tooltip("The Offset at which to follow the " + nameof(Target) + " Transform")]
        public Vector3 Offset = new Vector3(0f, 0f, -10f);

        private void Awake() {
            Assert.IsNotNull(TransformToMove, this.GetAssociationAssertion(nameof(this.TransformToMove)));
            Assert.IsNotNull(Target, this.GetAssociationAssertion(nameof(this.Target)));
        }
        private void Update() => TransformToMove.position = Target.position + Offset;

    }

}
