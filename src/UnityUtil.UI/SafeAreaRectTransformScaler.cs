using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.UI;

/// <summary>
/// Note that there is a simple <a href="https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html#Resolution">project setting</a>
/// for toggling safe area usage on Android (Player > Resolution and Presentation > Render outside safe area),
/// but <a href="https://forum.unity.com/threads/notch-avoidance-for-ios.1073261/">iOS doesn't support</a> this option.
/// This component can be used on iOS <em>and</em> Android for a consistent dev experience across platforms.
/// </summary>
public class SafeAreaRectTransformScaler : MonoBehaviour
{
    private ILogger<SafeAreaRectTransformScaler>? _logger;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public RectTransform? RectTransform;

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void ScaleRectTransform()
    {
        _logger!.CurrentSafeArea(RectTransform!);

        // Calculations inspired by this article: https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
        Rect safeArea = Screen.safeArea;
        var scaleVect = new Vector2(1f / Screen.width, 1f / Screen.height);
        RectTransform!.anchorMin = safeArea.position * scaleVect;
        RectTransform.anchorMax = (safeArea.position + safeArea.size) * scaleVect;

        _logger!.NewSafeArea(RectTransform);
    }
}
