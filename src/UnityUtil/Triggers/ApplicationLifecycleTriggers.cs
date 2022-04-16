using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;
using UnityEngine.Logging;

namespace UnityEngine.Triggers;

public class ApplicationLifecycleTriggers : Configurable
{
    private ILogger? _logger;

    public UnityEvent Focused = new();
    public UnityEvent Unfocused = new();
    public UnityEvent Paused = new();
    public UnityEvent Unpaused = new();
    public UnityEvent Quitting = new();

    public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationFocus(bool hasFocus)
    {
        _logger!.Log($"Application {(hasFocus ? "focused" : "blurred")}", context: this);
        (hasFocus ? Focused : Unfocused).Invoke();
    }

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationPause(bool pauseStatus)
    {
        _logger!.Log($"Application {(pauseStatus ? "" : "un")}paused", context: this);
        (pauseStatus ? Paused : Unpaused).Invoke();
    }

    [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnApplicationQuit()
    {
        _logger!.Log($"Application quitting...", context: this);
        Quitting.Invoke();
    }
}
