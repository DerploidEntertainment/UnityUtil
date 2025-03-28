// ----------------------------------------------------------------------------
// Inspired by Unite 2017 talk: "Game Architecture with Scriptable Objects"
// ----------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

public class ApplicationEventListener : MonoBehaviour
{
    [field: Required, SerializeField, ShowBackingField, Tooltip("Event to listen to")]
    public ApplicationEvent? Event { get; private set; }

    [Button, ShowInInspector]
    [Tooltip(
        "Invoke this component's handlers. " +
        $"To invoke the actual {nameof(ApplicationEvent)} that we're listening to (may call handlers on other components), call {nameof(invokeEvent)}."
    )]
    private void invokeHandlers() => _eventInvoked.Invoke();

    [Button, ShowInInspector]
    [Tooltip(
        $"Invoke the actual {nameof(ApplicationEvent)} that we're listening to (may call handlers on other components). " +
        $"To invoke only this component's handlers (equivalent to {nameof(_eventInvoked)} being invoked), call {nameof(invokeHandlers)}."
    )]
    private void invokeEvent() => Event!.Invoke();

    [field: SerializeField, ShowInInspector, LabelText(nameof(_eventInvoked), nicifyText: true)]
    [field: Tooltip($"Actions to invoke when {nameof(Event)} is invoked")]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Unity doesn't serialize readonly fields")]
    private UnityEvent _eventInvoked = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnEnable() => Event!.Invoked += doInvoke;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnDisable() => Event!.Invoked -= doInvoke;

    private void doInvoke(object sender, EventArgs e) => _eventInvoked.Invoke();
}
