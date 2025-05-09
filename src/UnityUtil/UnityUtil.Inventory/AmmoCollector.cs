﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collector))]
public class AmmoCollector : MonoBehaviour
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Inventory? Inventory;
    public float Radius = 1f;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => GetComponent<Collector>().Collected.AddListener(collect);

    private void collect(Collector collector, Collectible collectible)
    {
        // If no Ammo Collectible was found then just return
        AmmoCollectible ac = collectible.GetComponent<AmmoCollectible>();
        if (ac == null)
            return;

        // Try to find a Weapon with a matching name in the Inventory and adjust its ammo
        AmmoTool tool = Inventory!
            .GetComponentsInChildren<AmmoTool>(true)
            .SingleOrDefault(t => t.Info!.AmmoTypeName == ac.AmmoTypeName);
        if (tool != null) {
            int leftover = tool.Load((int)collectible.Amount);
            collectible.Collect(collector, leftover);
        }
    }
}
