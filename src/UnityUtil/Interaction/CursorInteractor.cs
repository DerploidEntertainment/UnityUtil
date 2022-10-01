using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Triggers;
using UnityUtil.Inputs;
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

        RegisterUpdatesAutomatically = true;
        BetterUpdate = raycastScreen;
    }

    private void raycastScreen(float deltaTime)
    {
        if (Input!.Started()) {
            Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
        }
    }

}
