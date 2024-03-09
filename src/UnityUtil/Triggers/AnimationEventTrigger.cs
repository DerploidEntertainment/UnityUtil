using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

[Serializable]
public class NamedAnimationEvent
{
    public string Name = "";
    public UnityEvent Trigger = new();
}

[RequireComponent(typeof(Animator))]
public class AnimationEventTrigger : MonoBehaviour
{

    private Dictionary<string, UnityEvent> _triggerDict = [];

    [SerializeField, LabelText("Triggers")]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Unity doesn't serialize readonly fields")]
    private NamedAnimationEvent[] _triggers = [];

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => _triggerDict = _triggers.ToDictionary(x => x.Name, x => x.Trigger);

    /// <summary>
    /// Warning! This method is not meant to be called programmatically.
    /// Instead, create an <see cref="AnimationClip"/> with an <see cref="AnimationEvent"/> that calls this method.
    /// </summary>
    /// <param name="eventName">Name of the event that was raised by the <see cref="Animator"/></param>
    public void Trigger(string eventName)
    {
        if (!_triggerDict.TryGetValue(eventName, out UnityEvent trigger)) {
            Debug.LogWarning($"No trigger associate with named AnimationEvent '{eventName}'");
            return;
        }

        if (trigger is null) {
            Debug.LogWarning($"Trigger associated with named AnimationEvent '{eventName}' cannot be null");
            return;
        }

        trigger.Invoke();
    }

}
