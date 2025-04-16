using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

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
    private ILogger<AnimationEventTrigger>? _logger;

    private Dictionary<string, UnityEvent> _triggerDict = [];

    [SerializeField, LabelText("Triggers")]
    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Unity doesn't serialize readonly fields")]
    private NamedAnimationEvent[] _triggers = [];

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        _triggerDict = _triggers.ToDictionary(x => x.Name, x => x.Trigger);
    }

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    /// <summary>
    /// Warning! This method is not meant to be called programmatically.
    /// Instead, create an <see cref="AnimationClip"/> with an <see cref="AnimationEvent"/> that calls this method.
    /// </summary>
    /// <param name="eventName">Name of the event that was raised by the <see cref="Animator"/></param>
    public void Trigger(string eventName)
    {
        if (!_triggerDict.TryGetValue(eventName, out UnityEvent trigger)) {
            log_NoTrigger(eventName);
            return;
        }

        if (trigger is null) {
            log_NullTrigger(eventName);
            return;
        }

        trigger.Invoke();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, Exception?> LOG_NO_TRIGGER_ACTION = LoggerMessage.Define<string>(Warning,
        new EventId(id: 0, nameof(log_NoTrigger)),
        "No trigger associated with AnimationEvent '{EventName}'"
    );
    private void log_NoTrigger(string eventName) => LOG_NO_TRIGGER_ACTION(_logger!, eventName, null);

    private static readonly Action<MEL.ILogger, string, Exception?> LOG_NULL_TRIGGER_ACTION = LoggerMessage.Define<string>(Warning,
        new EventId(id: 0, nameof(log_NullTrigger)),
        "Trigger associated with AnimationEvent '{EventName}' cannot be null"
    );
    private void log_NullTrigger(string eventName) => LOG_NULL_TRIGGER_ACTION(_logger!, eventName, null);

    #endregion
}
