using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Movement {
    
    public class TransformMover : MonoBehaviour {

        public Transform TransformToMove;
        public TransformMovement MovementData;

        private void Awake() =>
            Assert.IsNotNull(MovementData, $"{nameof(Movement.TransformMover)} was not associated with any {nameof(MovementData)}");

        private void Update() => MovementData.Move(TransformToMove);

    }

}
