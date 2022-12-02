using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;
using UnityUtil.Triggers;

namespace UnityUtil.UI;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is pretty familiar terminology")]
public class UiStack : MonoBehaviour
{
    private ILogger? _logger;
    private readonly Stack<SimpleTrigger> _popTriggers = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

    public void PushUi(SimpleTrigger popTrigger)
    {
        if (popTrigger == null) {
            _logger!.LogError($"A {nameof(popTrigger)} must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is later popped.", context: this);
            return;
        }

        _popTriggers.Push(popTrigger);
    }
    public void PopUi()
    {
        if (_popTriggers.Count == 0) {
            _logger!.LogWarning("No more UI to pop from stack", context: this);
            return;
        }

        SimpleTrigger popTrigger = _popTriggers.Pop();
        popTrigger.Trigger();
    }

}
