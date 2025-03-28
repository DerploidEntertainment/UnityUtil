using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

public class Collectible : MonoBehaviour
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public GameObject? Root;

    [Min(0)] public float Amount = 25f;
    public CollectibleDestroyMode DestroyMode = CollectibleDestroyMode.WhenUsed;
    public CollectEvent Detected = new();
    public CollectEvent Used = new();
    public CollectEvent Emptied = new();

    public void Collect(Collector collector, float newValue)
    {
        // Set the new amount
        float change = newValue - Amount;
        Amount = newValue;

        // Raise collection events, as necessary
        Detected.Invoke(collector, this);
        if (change != 0f)
            Used.Invoke(collector, this);
        if (newValue == 0f)
            Emptied.Invoke(collector, this);

        // Destroy the Root GameObject, if necessary
        if (
            (DestroyMode == CollectibleDestroyMode.WhenUsed && change != 0f) ||
            (DestroyMode == CollectibleDestroyMode.WhenEmptied && newValue == 0f) ||
            (DestroyMode == CollectibleDestroyMode.WhenDetected)) {
            Destroy(Root);
        }
    }

}
