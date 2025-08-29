using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Collector))]
public class QuantityCollector : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public ManagedQuantity? Quantity;

    private void Awake() => GetComponent<Collector>().Collected.AddListener(collect);

    private void collect(Collector collector, Collectible collectible)
    {
        // If no Quantity Collectible was found then just return
        if (!collectible.TryGetComponent(out QuantityCollectible qc))
            return;

        // If one was found, then adjust its current health as necessary
        float leftover = Quantity!.Increase(collectible.Amount, qc.ChangeMode);
        collectible.Collect(collector, leftover);
    }
}
