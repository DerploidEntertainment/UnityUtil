using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Updating;

namespace UnityUtil.Triggers;

public class InspectorButtonTrigger : Updatable
{
    private float _tRefactory = -1f;

    public UnityEvent Triggered = new();

    [Tooltip(
        "This flag will be set to false while in the refactory period, so that you don't accidentally press the button again. " +
        "You can manually tick it back to true if you want to press the button during a refactory period."
    )]
    public bool CanPress = true;

    [Tooltip("Time, in seconds, before the button may be pressed again.")]
    public float RefactoryPeriod = 1f;

    [Button, EnableIf(nameof(CanPress))]
    public void Press()
    {
        // Don't press the button if its still in the refractory period
        if (!CanPress)
            return;

        // Otherwise, raise the trigger event and prevent the button from being pressed for the desired period
        Triggered.Invoke();
        CanPress = false;
        _tRefactory = 0f;
        RegisterUpdate(updateRefactory);
    }

    private void updateRefactory(float deltaTime)
    {
        if (_tRefactory <= RefactoryPeriod) {
            _tRefactory += deltaTime;
            return;
        }

        _tRefactory = 0f;
        UnregisterUpdate();

        CanPress = true;
    }

}
