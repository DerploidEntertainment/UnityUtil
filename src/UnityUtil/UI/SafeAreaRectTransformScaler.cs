using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityEngine.UI;

/// <summary>
/// Note that there is a simple <a href="https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html#Resolution">project setting</a>
/// for toggling safe area usage on Android (Player > Resolution and Presentation > Render outside safe area),
/// but <a href="https://forum.unity.com/threads/notch-avoidance-for-ios.1073261/">iOS doesn't support</a> this option.
/// This component can be used on iOS <em>and</em> Android for a consistent dev experience across platforms.
/// </summary>
public class SafeAreaRectTransformScaler : MonoBehaviour
{
    private ILogger? _logger;

    [Required]
    public RectTransform? RectTransform;

    public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        Rect safeArea = Device.Screen.safeArea;
        _logger!.Log(
            $"Current anchors of {RectTransform!.GetHierarchyNameWithType()} (min, max): ({RectTransform!.anchorMin}, {RectTransform.anchorMax}). " +
            $"Updating for current screen (width x height) = ({Device.Screen.width} x {Device.Screen.height}) and safe area (width x height) = ({safeArea.width} x {safeArea.height})"
        , context: this);

        // Calculations inspired by this article: https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
        var scaleVect = new Vector2(1f / Device.Screen.width, 1f / Device.Screen.height);
        RectTransform.anchorMin = safeArea.position * scaleVect;
        RectTransform.anchorMax = (safeArea.position + safeArea.size) * scaleVect;

        _logger!.Log($"New anchors of {RectTransform.GetHierarchyNameWithType()} (min, max): ({RectTransform.anchorMin}, {RectTransform.anchorMax})");
    }
}
