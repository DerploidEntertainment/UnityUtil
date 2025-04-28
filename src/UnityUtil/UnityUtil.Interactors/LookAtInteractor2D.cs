using UnityEngine;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interactors;

public class LookAtInteractor2D : Updatable
{

    private ToggleTrigger? _trigger;

    public float Range;
    public LayerMask InteractLayerMask;

    protected override void Awake()
    {
        base.Awake();

        AddUpdate(look);
    }

    private void look(float deltaTime)
    {
        RaycastHit2D hit = U.Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
        ToggleTrigger? trigger = hit.collider?.GetComponent<ToggleTrigger>();
        if (trigger is null)
            _trigger?.TurnOff();
        else if (_trigger is null) {
            _trigger = trigger;
            _trigger.TurnOn();
        }
        else if (trigger != _trigger) {
            _trigger.TurnOff();
            _trigger = trigger;
            _trigger.TurnOn();
        }
    }

}
