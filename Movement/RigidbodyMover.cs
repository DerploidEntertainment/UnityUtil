using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Movement {
    
    public class RigidbodyMover : MonoBehaviour {

        public Rigidbody RigidbodyToMove;
        public RigidbodyMovement MovementData;

        private void Awake() {
            Assert.IsNotNull(MovementData, $"{nameof(Movement.RigidbodyMover)} was not associated with any {nameof(MovementData)}");
        }

        private void FixedUpdate() {
            MovementData.Move(RigidbodyToMove);
        }

    }

}
