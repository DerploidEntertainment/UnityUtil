using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace UnityEngine.Triggers;

public abstract class ConditionalTrigger : MonoBehaviour
{
    public abstract bool IsConditionMet();

    protected const string ButtonGroup = "Buttons";
    private const string TRUE_MSG =
        $"{nameof(BecameTrue)} is raised if the trigger's condition WAS false and BECAME true. " +
        $"{nameof(StillTrue)} is raised if the trigger's condition WAS true and STILL IS true (e.g., during a call to {nameof(TriggerState)}.";
    private const string FALSE_MSG =
        $"{nameof(BecameFalse)} is raised if the trigger's condition WAS true and BECAME false. " +
        $"{nameof(StillFalse)} is raised if the trigger's condition WAS false and STILL IS false (e.g., during a call to {nameof(TriggerState)}.";

    [Button, HorizontalGroup(ButtonGroup)]
    public void TriggerState()
    {
        if (RaiseStillEvents)
            (IsConditionMet() ? StillTrue : StillFalse).Invoke();
    }

    [ToggleGroup(nameof(RaiseBecameEvents), nameof(RaiseBecameEvents))]
    public bool RaiseBecameEvents = true;

    [ToggleGroup(nameof(RaiseBecameEvents), nameof(RaiseBecameEvents)), Tooltip(TRUE_MSG)]
    public UnityEvent BecameTrue = new();

    [ToggleGroup(nameof(RaiseBecameEvents), nameof(RaiseBecameEvents)), Tooltip(FALSE_MSG)]
    public UnityEvent BecameFalse = new();

    [ToggleGroup(nameof(RaiseStillEvents), nameof(RaiseStillEvents))]
    public bool RaiseStillEvents = true;

    [ToggleGroup(nameof(RaiseStillEvents), nameof(RaiseStillEvents)), Tooltip(TRUE_MSG)]
    public UnityEvent StillTrue = new();

    [ToggleGroup(nameof(RaiseStillEvents), nameof(RaiseStillEvents)), Tooltip(FALSE_MSG)]
    public UnityEvent StillFalse = new();

}
