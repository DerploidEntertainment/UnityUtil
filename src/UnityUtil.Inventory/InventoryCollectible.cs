using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UnityUtil.Inventory;

public class InventoryCollectible : MonoBehaviour
{
    [Required] public GameObject? Root;
    [Required] public GameObject? ItemRoot;

    [Tooltip(
        "These should be the Colliders that allow your Collectible to be collected. " +
        "When dropped, these Colliders will be enabled/disabled to create the refactory period."
    )]
    public Collider[] CollidersToToggle = [];
}
