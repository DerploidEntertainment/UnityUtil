using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Movement;

public class RigidbodyMover : MonoBehaviour
{

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Rigidbody? RigidbodyToMove;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public RigidbodyMovement? MovementData;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void FixedUpdate() => MovementData!.Move(RigidbodyToMove!);

}
