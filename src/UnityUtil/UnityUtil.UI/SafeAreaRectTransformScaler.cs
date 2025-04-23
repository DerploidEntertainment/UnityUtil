using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

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
        log_CurrentSafeArea(RectTransform!);

        // Calculations inspired by this article: https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
        Rect safeArea = Screen.safeArea;
        var scaleVect = new Vector2(1f / Screen.width, 1f / Screen.height);
        RectTransform!.anchorMin = safeArea.position * scaleVect;
        RectTransform.anchorMax = (safeArea.position + safeArea.size) * scaleVect;

        log_NewSafeArea(RectTransform);
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, string, string, string, Exception?> LOG_CURR_SAFE_AREA_ACTION =
        LoggerMessage.Define<string, string, string, string>(Information,
            new EventId(id: 0, nameof(log_CurrentSafeArea)),
            "Current anchors of {RectTransform}: ({Anchors}). " +
            "Updating for current screen ({ScreenDimensions}) and safe area ({SafeAreaDimensions})."
        );
    private void log_CurrentSafeArea(RectTransform rectTransform) =>
        // We can only pass up to 6 params to Actions created by LoggerMessage.Define().
        // Methods with the [LoggerMessage] attr could take more, but that isn't available until MEL 6.0.0: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6#microsoftextensions-apis
        // And we want to depend on the lowest MEL version possible.
        LOG_CURR_SAFE_AREA_ACTION(_logger!,
            rectTransform.GetHierarchyNameWithType(),
            $"({rectTransform.anchorMin}, {rectTransform.anchorMax})",
            $"({Screen.width} x {Screen.height})",
            $"({Screen.safeArea.width} x {Screen.safeArea.height})",
            null
        );


    private static readonly Action<MEL.ILogger, string, string, Exception?> LOG_NEW_SAFE_AREA_ACTION =
        LoggerMessage.Define<string, string>(Information, new EventId(id: 0, nameof(log_NewSafeArea)), "New anchors of {RectTransform}: {Anchors}");
    private void log_NewSafeArea(RectTransform rectTransform) =>
        LOG_NEW_SAFE_AREA_ACTION(_logger!,
            rectTransform.GetHierarchyNameWithType(),
            $"({rectTransform.anchorMin}, {rectTransform.anchorMax})",
            null
        );

    #endregion
}
