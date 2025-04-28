using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;
using UnityUtil.Triggers;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.UI;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is pretty familiar terminology")]
public class UiStack : MonoBehaviour
{
    private ILogger<UiStack>? _logger;
    private readonly Stack<SimpleTrigger> _popTriggers = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    public void PushUi(SimpleTrigger popTrigger)
    {
        if (popTrigger == null) {   // Null could be passed via UnityEvents in Editor
            log_UiStackPushNullTrigger();
            return;
        }

        _popTriggers.Push(popTrigger);
    }
    public void PopUi()
    {
        if (_popTriggers.Count == 0)
            return;

        SimpleTrigger popTrigger = _popTriggers.Pop();
        popTrigger.Trigger();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_PUSH_NULL_TRIGGER_ACTION =
        LoggerMessage.Define(Information,
            new EventId(id: 0, nameof(log_UiStackPushNullTrigger)),
            "A trigger must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is popped later"
        );
    private void log_UiStackPushNullTrigger() => LOG_PUSH_NULL_TRIGGER_ACTION(_logger!, null);

    #endregion
}
