using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventories;

[RequireComponent(typeof(Collectible))]
public class AmmoCollectible : MonoBehaviour
{
    public string AmmoTypeName = "";

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() =>
        Assert.IsFalse(string.IsNullOrEmpty(AmmoTypeName), $"{this.GetHierarchyNameWithType()} must specify a value for {nameof(AmmoTypeName)}!");

}
