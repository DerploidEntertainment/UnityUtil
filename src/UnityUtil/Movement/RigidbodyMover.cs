using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Logging;

namespace UnityEngine.Movement {

    public class RigidbodyMover : MonoBehaviour {

        public Rigidbody RigidbodyToMove;
        public RigidbodyMovement MovementData;

        private void Awake() => this.AssertAssociation(MovementData, nameof(this.MovementData));

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void FixedUpdate() => MovementData.Move(RigidbodyToMove);

    }

}
