using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Physics;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class PhysTarget : MonoBehaviour
{
    [Required]
    public MonoBehaviour? TargetComponent;
}
