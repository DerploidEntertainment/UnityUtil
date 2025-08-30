using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Inventory;

/// <summary>
/// Type arguments are (Collector collector, Collectible collectible)
/// </summary>
[Serializable]
public class CollectEvent : UnityEvent<Collector, Collectible> { }

public class Collector : MonoBehaviour
{
    public float Radius = 1f;
    public CollectEvent Collected = new();

    protected virtual void Awake()
    {
        SphereCollider? sphere = gameObject.AddComponent<SphereCollider>();
        sphere.radius = Radius;
        sphere.isTrigger = true;
    }

    private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);

    private void OnTriggerEnter(Collider other)
    {
        // If no collectible was found then just return
        if (other.attachedRigidbody.TryGetComponent(out Collectible c))
            Collected.Invoke(this, c);
    }

}
