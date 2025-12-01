using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interactors;

public class CursorInteractor : Updatable
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
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask)
                && hitInfo.collider != null
                && hitInfo.collider.TryGetComponent(out SimpleTrigger trigger)
            )
                trigger.Trigger();
        }
    }

}
