using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Movement {

    public class RigidbodyMover : MonoBehaviour {

        public Rigidbody RigidbodyToMove;
        public RigidbodyMovement MovementData;

        private void Awake() => Assert.IsNotNull(MovementData, this.GetAssociationAssertion(nameof(this.MovementData)));

        private void FixedUpdate() => MovementData.Move(RigidbodyToMove);

    }

}
