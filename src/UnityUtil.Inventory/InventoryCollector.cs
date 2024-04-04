﻿using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil.Inventory;

public class InventoryCollector : MonoBehaviour
{
    private SphereCollider? _sphere;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Inventory? Inventory;
    public float Radius = 1f;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        _sphere = gameObject.AddComponent<SphereCollider>();
        _sphere.radius = Radius;
        _sphere.isTrigger = true;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnTriggerEnter(Collider other)
    {
        InventoryCollectible c = other.attachedRigidbody.GetComponent<InventoryCollectible>();
        if (c != null)
            Inventory!.Collect(c);
    }

}
