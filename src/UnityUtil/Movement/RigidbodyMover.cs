using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine.Movement;

public class RigidbodyMover : MonoBehaviour
{

    [Required] public Rigidbody? RigidbodyToMove;
    [Required] public RigidbodyMovement? MovementData;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void FixedUpdate() => MovementData!.Move(RigidbodyToMove!);

}
