using UnityEngine;

namespace Danware.Unity.Movement {
    
    public abstract class TransformMovement : MonoBehaviour {

        public void Move(Transform transform) => doMove(transform);
        protected abstract void doMove(Transform transform);

    }

}
