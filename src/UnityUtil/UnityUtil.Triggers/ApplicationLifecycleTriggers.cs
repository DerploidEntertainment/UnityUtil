using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Triggers;

public class ApplicationLifecycleTriggers : MonoBehaviour
{
    private ILogger<ApplicationLifecycleTriggers>? _logger;

    public UnityEvent Focused = new();
    public UnityEvent Unfocused = new();
    public UnityEvent Paused = new();
    public UnityEvent Unpaused = new();
    public UnityEvent Quitting = new();

    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    private void OnApplicationFocus(bool hasFocus)
    {
        log_FocusChanged(hasFocus);
        (hasFocus ? Focused : Unfocused).Invoke();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        log_PauseChanged(pauseStatus);
        (pauseStatus ? Paused : Unpaused).Invoke();
    }

    private void OnApplicationQuit()
    {
        log_Quitting();
        Quitting.Invoke();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, bool, Exception?> LOG_FOCUS_CHANGED_ACTION =
        LoggerMessage.Define<bool>(Information, new EventId(id: 0, nameof(log_FocusChanged)), "Application focus updated to {IsFocused}");
    private void log_FocusChanged(bool isFocused) => LOG_FOCUS_CHANGED_ACTION(_logger!, isFocused, null);


    private static readonly Action<MEL.ILogger, bool, Exception?> LOG_PAUSE_CHANGED_ACTION =
        LoggerMessage.Define<bool>(Information, new EventId(id: 0, nameof(log_PauseChanged)), "Application pause state updated to {IsPaused}");
    private void log_PauseChanged(bool isPaused) => LOG_PAUSE_CHANGED_ACTION(_logger!, isPaused, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_QUITTING_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Quitting)), "Application quitting...");
    private void log_Quitting() => LOG_QUITTING_ACTION(_logger!, null);

    #endregion
}
