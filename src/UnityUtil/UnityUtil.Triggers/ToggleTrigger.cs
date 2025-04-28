using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Triggers;

public class ToggleTrigger : ConditionalTrigger
{
    [PropertyOrder(-5f), ReadOnly, ShowInInspector]
    [Tooltip("Note that this property may not refresh unless you select a different component in the Inspector then select this component again.")]
    private bool _currentState;

    [PropertyOrder(-4f)]
    public bool AwakeState = false;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => _currentState = AwakeState;

    [Button, PropertyOrder(-3f), HorizontalGroup(ButtonGroup)]
    public void TurnOn()
    {
        if (!_currentState) {
            _currentState = true;
            BecameTrue.Invoke();
        }
        else
            StillTrue.Invoke();
    }
    [Button, PropertyOrder(-2f), HorizontalGroup(ButtonGroup)]
    public void Toggle()
    {
        _currentState = !_currentState;
        (_currentState ? BecameTrue : BecameFalse).Invoke();
    }
    [Button, PropertyOrder(-1f), HorizontalGroup(ButtonGroup)]
    public void TurnOff()
    {
        if (_currentState) {
            _currentState = false;
            BecameFalse.Invoke();
        }
        else
            StillFalse.Invoke();
    }

    public override bool IsConditionMet() => _currentState;

}
