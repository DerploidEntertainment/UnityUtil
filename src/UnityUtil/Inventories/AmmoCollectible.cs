using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventories;

[RequireComponent(typeof(Collectible))]
public class AmmoCollectible : MonoBehaviour
{
    [Required]
    public string AmmoTypeName = "";
}
