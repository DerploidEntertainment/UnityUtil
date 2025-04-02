using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Audio;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.UI;

/// <inheritdoc/>
internal static class UiLoggerExtensions
{
    #region Information

    public static void AudioMixerParameterFromPrefs(this MEL.ILogger logger, float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        logger.LogInformation(new EventId(id: 0, nameof(AudioMixerParameterFromPrefs)), $"Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} from {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public static void AudioMixerParameterFromInspector(this MEL.ILogger logger, float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        logger.LogInformation(new EventId(id: 0, nameof(AudioMixerParameterFromInspector)), $"Not using local preferences or key {{{nameof(preferencesKey)}}} could not be found. Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} directly from {{{nameof(audioMixer)}}} instead", preferencesKey, value, parameter, audioMixer);

    public static void AudioMixerParameterValueSaved(this MEL.ILogger logger, float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        logger.LogInformation(new EventId(id: 0, nameof(AudioMixerParameterValueSaved)), $"Saved {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} to {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public static void AudioMixerParameterPrefDeleted(this MEL.ILogger logger, string preferencesKey) =>
        logger.LogInformation(new EventId(id: 0, nameof(AudioMixerParameterPrefDeleted)), $"Deleted {{{nameof(preferencesKey)}}}", preferencesKey);

    public static void CurrentSafeArea(this MEL.ILogger logger, RectTransform rectTransform) =>
        logger.LogInformation(new EventId(id: 0, nameof(CurrentSafeArea)),
            $"Current anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}}). " +
            $"Updating for current screen ({{screenWidth}} x {{screenHeight}}) and safe area ({{safeAreaWidth}} x {{safeAreaHeight}}).",
            rectTransform.GetHierarchyNameWithType(),
            rectTransform.anchorMin, rectTransform.anchorMax,
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height
        );

    public static void NewSafeArea(this MEL.ILogger logger, RectTransform rectTransform) =>
        logger.LogInformation(new EventId(id: 0, nameof(NewSafeArea)),
            $"New anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}})",
            rectTransform.GetHierarchyNameWithType(), rectTransform.anchorMin, rectTransform.anchorMax
        );

    public static void UiBreakpointUpdating(this MEL.ILogger logger, BreakpointMode mode, BreakpointMatchMode matchMode) =>
        logger.LogInformation(new EventId(id: 0, nameof(UiBreakpointUpdating)),
            $"Current screen dimensions are ({{screenWidth}} x {{screenHeight}}) (screen), ({{safeAreaWidth}} x {{safeAreaHeight}}) (safe area). " +
            $"Updating breakpoints with {{{nameof(mode)}}} and {{{nameof(matchMode)}}}...",
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height,
            mode, matchMode
        );

    public static void SplashScreenInitializing(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(SplashScreenInitializing)), "Initializing splash screen...");

    public static void SplashScreenDrawing(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(SplashScreenDrawing)), "Starting to draw splash screen...");

    public static void SplashScreenStopping(this MEL.ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(SplashScreenStopping)), "Stopping splash screen...");

    #endregion

    #region Warning

    public static void UiBreakpointNegativeModeValue(this MEL.ILogger logger) =>
        logger.LogWarning(new EventId(id: 0, nameof(UiBreakpointNegativeModeValue)), "UI breakpoints can only be matched against non-negative values. You can ignore this warning if you just restarted the Editor.");

    #endregion

    #region Error

    public static void UiStackPushNullTrigger(this MEL.ILogger logger) =>
        logger.LogError(new EventId(id: 0, nameof(UiStackPushNullTrigger)), $"A trigger must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is popped later");

    #endregion
}
