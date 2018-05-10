using UnityEngine;

namespace UnityEngine.Movement {

    public abstract class RigidbodyMovement : MonoBehaviour {

        public void Move(Rigidbody rb) => doMove(rb);
        protected abstract void doMove(Rigidbody rb);

    }

}
