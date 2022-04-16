using Sirenix.OdinInspector;
using UnityEngine.Triggers;
using U = UnityEngine;

namespace UnityEngine.Inputs;

public class CursorInteractor2D : Updatable
{
    public LayerMask InteractLayerMask;

    [Required]
    public StartStopInput? Input;

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdatesAutomatically = true;
        BetterUpdate = raycastScreen;
    }

    private void raycastScreen(float deltaTime)
    {
        if (Input!.Started()) {
            Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
            hit.collider?.GetComponent<SimpleTrigger>()?.Trigger();
        }
    }

}
