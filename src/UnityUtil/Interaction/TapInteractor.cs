using UnityEngine;
using UnityEngine.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interaction;

public class TapInteractor : Updatable
{

    public LayerMask InteractLayerMask;

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdatesAutomatically = true;
        BetterUpdate = tap;
    }

    private void tap(float deltaTime)
    {
        if (Input.touchCount == 1) {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            if (U.Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
        }
    }

}
