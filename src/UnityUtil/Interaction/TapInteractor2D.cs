using UnityEngine.Triggers;
using UnityUtil.Updating;

namespace UnityEngine.Inputs;

public class TapInteractor2D : Updatable
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
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
            hit.collider?.GetComponent<SimpleTrigger>()?.Trigger();
        }
    }
}
