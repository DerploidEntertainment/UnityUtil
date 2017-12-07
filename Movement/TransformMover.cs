using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Movement {

    public class TransformMover : MonoBehaviour {

        public Transform TransformToMove;
        public TransformMovement MovementData;

        private void Awake() => Assert.IsNotNull(MovementData, this.GetAssociationAssertion(nameof(this.MovementData)));

        private void Update() => MovementData.Move(TransformToMove);

    }

}
