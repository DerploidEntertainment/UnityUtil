using UnityEngine;

namespace Danware.Unity.Movement {
    
    public class TargetOffsetMovement : TransformMovement {

        // INSPECTOR FIELDS
        [Tooltip("The Target being followed at the given Offset")]
        public Transform Target;
        [Tooltip("The Offset at which to follow the Target")]
        public Vector3 Offset = new Vector3(0f, 0f, -10f);

        protected override void doMove(Transform transform) => transform.position = Target.position + Offset;

    }

}
