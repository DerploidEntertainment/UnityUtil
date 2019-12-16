using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Logging;

namespace UnityEngine.Movement {

    public class RigidbodyMover : MonoBehaviour {

        public Rigidbody RigidbodyToMove;
        public RigidbodyMovement MovementData;

        private void Awake() => this.AssertAssociation(MovementData, nameof(this.MovementData));

        private void FixedUpdate() => MovementData.Move(RigidbodyToMove);

    }

}
