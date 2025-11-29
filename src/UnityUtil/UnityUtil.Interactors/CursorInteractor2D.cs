using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interactors;

public class CursorInteractor2D : Updatable
{
    public LayerMask InteractLayerMask;

    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public StartStopInput? Input;

    protected override void Awake()
    {
        base.Awake();

        AddUpdate(raycastScreen);
    }

    private void raycastScreen(float deltaTime)
    {
        if (Input!.Started()) {
            Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
            if (hit.collider != null && hit.collider.TryGetComponent(out SimpleTrigger trigger))
                trigger.Trigger();
        }
    }

}
