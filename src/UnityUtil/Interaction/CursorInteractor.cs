using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Interaction;

public class CursorInteractor : Updatable
{
    public LayerMask InteractLayerMask;

    [Required]
    public StartStopInput? Input;

    protected override void Awake()
    {
        base.Awake();

        UpdateAction = raycastScreen;
    }

    private void raycastScreen(float deltaTime)
    {
        if (Input!.Started()) {
            Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
            if (U.Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
        }
    }

}
