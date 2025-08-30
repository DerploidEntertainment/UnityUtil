using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collector))]
public class AmmoCollector : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public Inventory? Inventory;
    public float Radius = 1f;

    private void Awake() => GetComponent<Collector>().Collected.AddListener(collect);

    private void collect(Collector collector, Collectible collectible)
    {
        // If no Ammo Collectible was found then just return
        if (!collectible.TryGetComponent(out AmmoCollectible ac))
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
