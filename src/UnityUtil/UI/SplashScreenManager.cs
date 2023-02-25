using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityUtil.DependencyInjection;

namespace UnityUtil.UI;

public class SplashScreenManager : MonoBehaviour, ISplashScreenManager
{
    private UiLogger<SplashScreenManager>? _logger;

    public SplashScreen.StopBehavior StopBehavior;

    public UnityEvent StartedDrawing = new();
    public UnityEvent StoppedDrawing = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

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
