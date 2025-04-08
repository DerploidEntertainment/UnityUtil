using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collector))]
public class QuantityCollector : MonoBehaviour
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public ManagedQuantity? Quantity;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        GetComponent<Collector>().Collected.AddListener(collect);
    }
    private void collect(Collector collector, Collectible collectible)
    {
        // If no Quantity Collectible was found then just return
        QuantityCollectible qc = collectible.GetComponent<QuantityCollectible>();
        if (qc == null)
            return;

        // If one was found, then adjust its current health as necessary
        float leftover = Quantity!.Increase(collectible.Amount, qc.ChangeMode);
        collectible.Collect(collector, leftover);
    }
}
