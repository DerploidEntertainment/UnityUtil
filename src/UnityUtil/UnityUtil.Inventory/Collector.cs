using System;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnTriggerEnter(Collider other)
    {
        // If no collectible was found then just return
        Collectible c = other.attachedRigidbody.GetComponent<Collectible>();
        if (c != null)
            Collected.Invoke(this, c);
    }

}
