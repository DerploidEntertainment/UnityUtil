using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Triggers;

public class ApplicationLifecycleTriggers : MonoBehaviour
{
    private TriggersLogger<ApplicationLifecycleTriggers>? _logger;

    public UnityEvent Focused = new();
    public UnityEvent Unfocused = new();
    public UnityEvent Paused = new();
    public UnityEvent Unpaused = new();
    public UnityEvent Quitting = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory!, context: this);

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationFocus(bool hasFocus)
    {
        _logger!.ApplicationFocusChanged(hasFocus);
        (hasFocus ? Focused : Unfocused).Invoke();
    }

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationPause(bool pauseStatus)
    {
        _logger!.ApplicationPauseChanged(pauseStatus);
        (pauseStatus ? Paused : Unpaused).Invoke();
    }

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationQuit()
    {
        _logger!.ApplicationQuitting();
        Quitting.Invoke();
    }
}
