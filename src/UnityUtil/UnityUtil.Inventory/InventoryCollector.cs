using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

public class InventoryCollector : MonoBehaviour
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public Inventory? Inventory;
    public float Radius = 1f;

    private void Awake()
    {
        SphereCollider? sphere = gameObject.AddComponent<SphereCollider>();
        sphere.radius = Radius;
        sphere.isTrigger = true;
    }

    private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody.TryGetComponent(out InventoryCollectible c))
            Inventory!.Collect(c);
    }

}
