using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collectible))]
public class AmmoCollectible : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public string AmmoTypeName = "";
}
