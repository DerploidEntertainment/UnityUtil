using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;

namespace UnityUtil.Interactors;

public class InteractionEventArgs : EventArgs
{
    public RaycastHit HitInfo;
    public SimpleTrigger? InteractedTrigger;
}

public class StartInteractor : Updatable
{
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
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
            if (
                Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask, QueryTriggerInteraction)
                && hitInfo.collider != null
                && hitInfo.collider.TryGetComponent(out SimpleTrigger trigger)
            ) {
                trigger.Trigger();
                Interacted?.Invoke(this, new InteractionEventArgs() { HitInfo = hitInfo, InteractedTrigger = trigger });
            }
        }
    }

}
