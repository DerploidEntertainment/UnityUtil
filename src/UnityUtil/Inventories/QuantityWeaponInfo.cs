﻿using System;
using UnityEngine;

namespace UnityUtil.Inventories;

[CreateAssetMenu(fileName = "quantity-weapon", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inventories)}/{nameof(QuantityWeaponInfo)}")]
public class QuantityWeaponInfo : ScriptableObject
{
    [Tooltip(
        $"Attacked {nameof(ManagedQuantity)}s will be changed by this amount. " +
        $"How this amount is applied depends on the value of {nameof(ChangeMode)}."
    )]
    public float Amount = 10f;

    [Tooltip($"Determines how the value of {nameof(Amount)} is used to change attacked {nameof(ManagedQuantity)}s.")]
    public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;

    [Tooltip(
        $"If true, then only the closest {nameof(ManagedQuantity)} attacked by this {nameof(QuantityWeapon)} will be changed. " +
        $"If false, then all attacked {nameof(ManagedQuantity)}s will be changed."
    )]
    public bool OnlyAffectClosest = true;

    [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
    public string[] IgnoreColliderTags = Array.Empty<string>();
}
