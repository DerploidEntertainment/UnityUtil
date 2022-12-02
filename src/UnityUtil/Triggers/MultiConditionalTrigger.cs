using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UnityUtil.Triggers;

public abstract class MultiConditionalTrigger : ConditionalTrigger
{
    [PropertyOrder(-5f), ReadOnly, ShowInInspector]
    [Tooltip("Note that this property may not refresh unless you select a different component in the Inspector then select this component again.")]
    private bool _currentState;

    public ConditionalTrigger[] Conditions = Array.Empty<ConditionalTrigger>();

    protected virtual void Awake() => ResetBecameEventListeners();

    internal void ResetBecameEventListeners()
    {
        if (Conditions is null)
            return;

        foreach (ConditionalTrigger condition in Conditions) {
            if (condition != null) {
                condition.BecameTrue.AddListener(handleBecameEvent);
                condition.BecameFalse.AddListener(handleBecameEvent);
            }
        }

        void handleBecameEvent()
        {
            bool oldState = _currentState;
            _currentState = IsConditionMet();
            if (RaiseBecameEvents && _currentState != oldState)
                (_currentState ? BecameTrue : BecameFalse).Invoke();
        }
    }

}
