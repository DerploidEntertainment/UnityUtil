using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interaction;

public class Interaction2DEventArgs : EventArgs
{
    public RaycastHit2D HitInfo;
    public SimpleTrigger? InteractedTrigger;
}

public class StartInteractor2D : Updatable
{
    [Required]
    public StartStopInput? Input;
    public float Range;
    public LayerMask InteractLayerMask;

    public event EventHandler<Interaction2DEventArgs>? Interacted;

    protected override void Awake()
    {
        base.Awake();

        UpdateAction = raycast;
    }

    private void raycast(float deltaTime)
    {
        if (Input!.Started()) {
            RaycastHit2D hit = U.Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
            if (hit.collider != null) {
                SimpleTrigger st = hit.collider.GetComponent<SimpleTrigger>();
                st?.Trigger();
                Interacted?.Invoke(this, new Interaction2DEventArgs() { HitInfo = hit, InteractedTrigger = st });
            }
        }
    }

}
