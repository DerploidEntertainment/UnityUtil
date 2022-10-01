using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Triggers;
using UnityUtil.Inputs;
using UnityUtil.Updating;

namespace UnityUtil.Interaction;

public class InteractionEventArgs : EventArgs
{
    public RaycastHit HitInfo;
    public SimpleTrigger? InteractedTrigger;
}

public class StartInteractor : Updatable
{
    [Required]
    public StartStopInput? Input;
    public float Range;
    public LayerMask InteractLayerMask;
    public QueryTriggerInteraction QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

    public event EventHandler<InteractionEventArgs>? Interacted;

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdatesAutomatically = true;
        BetterUpdate = raycast;
    }

    private void raycast(float deltaTime)
    {
        if (Input!.Started()) {
            bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask, QueryTriggerInteraction);
            if (somethingHit) {
                SimpleTrigger? st = hitInfo.collider.GetComponent<SimpleTrigger>();
                st?.Trigger();
                Interacted?.Invoke(this, new InteractionEventArgs() { HitInfo = hitInfo, InteractedTrigger = st });
            }
        }
    }

}
