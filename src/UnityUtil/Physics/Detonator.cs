﻿using System;
using UnityEngine;
using UnityEngine.Events;
using U = UnityEngine;

namespace UnityUtil.Physics;

[Serializable]
public class DetonateEvent : UnityEvent<Collider[]> { }

public class Detonator : MonoBehaviour
{
    public float ExplosionRadius = 4f;
    public LayerMask AffectLayerMask;

    public CancellableUnityEvent Detonating = new();
    public DetonateEvent Detonated = new();

    public void Detonate()
    {
        // Raise the Detonating event, allowing listeners to cancel detonation
        Detonating.Invoke();
        if (Detonating.Cancel)
            return;

        // Do an OverlapSphere into the scene on the given Affect Layer
        // Raise the Detonated event, allowing other components to select which targets to affect
        Collider[] hits = U.Physics.OverlapSphere(transform.position, ExplosionRadius, AffectLayerMask);
        Detonated.Invoke(hits);
    }

}
