using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collectible))]
public class AmmoCollectible : MonoBehaviour
{
    [Required]
    public string AmmoTypeName = "";
}
