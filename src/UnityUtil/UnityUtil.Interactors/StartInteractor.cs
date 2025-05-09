﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interactors;

public class InteractionEventArgs : EventArgs
{
    public RaycastHit HitInfo;
    public SimpleTrigger? InteractedTrigger;
}

public class StartInteractor : Updatable
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? Input;
    public float Range;
    public LayerMask InteractLayerMask;
    public QueryTriggerInteraction QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

    public event EventHandler<InteractionEventArgs>? Interacted;

    protected override void Awake()
    {
        base.Awake();

        AddUpdate(raycast);
    }

    private void raycast(float deltaTime)
    {
        if (Input!.Started()) {
            bool somethingHit = U.Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask, QueryTriggerInteraction);
            if (somethingHit) {
                SimpleTrigger? st = hitInfo.collider.GetComponent<SimpleTrigger>();
                st?.Trigger();
                Interacted?.Invoke(this, new InteractionEventArgs() { HitInfo = hitInfo, InteractedTrigger = st });
            }
        }
    }

}
