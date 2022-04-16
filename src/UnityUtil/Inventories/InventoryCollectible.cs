﻿using Sirenix.OdinInspector;
using System;

namespace UnityEngine.Inventories;

public class InventoryCollectible : MonoBehaviour
{
    [Required] public GameObject? Root;
    [Required] public GameObject? ItemRoot;

    [Tooltip(
        "These should be the Colliders that allow your Collectible to be collected. " +
        "When dropped, these Colliders will be enabled/disabled to create the refactory period."
    )]
    public Collider[] CollidersToToggle = Array.Empty<Collider>();
}
