using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Movement;

public class RigidbodyMover : MonoBehaviour
{

    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public Rigidbody? RigidbodyToMove;

    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public RigidbodyMovement? MovementData;

    private void FixedUpdate() => MovementData!.Move(RigidbodyToMove!);

}
