using Sirenix.OdinInspector;

namespace UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class PhysTarget : MonoBehaviour
{
    [Required]
    public MonoBehaviour? TargetComponent;
}
