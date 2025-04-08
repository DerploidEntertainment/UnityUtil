using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityUtil.DependencyInjection;

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
        _logger!.SplashScreenInitializing();
        SplashScreen.Begin();
    }

    public void Draw()
    {
        _logger!.SplashScreenDrawing();
        SplashScreen.Draw();
        StartedDrawing.Invoke();
    }

    public void Stop()
    {
        _logger!.SplashScreenStopping();
        SplashScreen.Stop(StopBehavior);
        StoppedDrawing.Invoke();
    }
}
