using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.UI;

public class SplashScreenManager : MonoBehaviour, ISplashScreenManager
{
    private ILogger<SplashScreenManager>? _logger;

    [Tooltip(
        $"The behavior to apply when calling {nameof(Stop)}. " +
        $"See https://docs.unity3d.com/ScriptReference/Rendering.SplashScreen.StopBehavior.html."
    )]
    public SplashScreen.StopBehavior StopBehavior;

    public UnityEvent StartedDrawing = new();
    public UnityEvent StoppedDrawing = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    public void Begin()
    {
        log_Initializing();
        SplashScreen.Begin();
    }

    public void Draw()
    {
        log_Drawing();
        SplashScreen.Draw();
        StartedDrawing.Invoke();
    }

    public void Stop()
    {
        log_Stopping();
        SplashScreen.Stop(StopBehavior);
        StoppedDrawing.Invoke();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_INITIALIZING_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Initializing)), "Initializing splash screen...");
    private void log_Initializing() => LOG_INITIALIZING_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_DRAWING_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Drawing)), "Starting to draw splash screen...");
    private void log_Drawing() => LOG_DRAWING_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_STOPPING_ACTION =
        LoggerMessage.Define(Information, new EventId(id: 0, nameof(log_Stopping)), "Stopping splash screen...");
    private void log_Stopping() => LOG_STOPPING_ACTION(_logger!, null);

    #endregion
}
