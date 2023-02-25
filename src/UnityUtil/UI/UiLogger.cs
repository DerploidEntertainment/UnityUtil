using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Audio;
using UnityUtil.Logging;

namespace UnityUtil.UI;

/// <inheritdoc/>
internal class UiLogger<T> : BaseUnityUtilLogger<T>
{
    public UiLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 14_000) { }

    #region Information

    public void AudioMixerParameterFromPrefs(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        LogInformation(id: 0, nameof(AudioMixerParameterFromPrefs), $"Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} from {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public void AudioMixerParameterFromInspector(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        LogInformation(id: 1, nameof(AudioMixerParameterFromInspector), $"Not using local preferences or key {{{nameof(preferencesKey)}}} could not be found. Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} directly from {{{nameof(audioMixer)}}} instead", preferencesKey, value, parameter, audioMixer);

    public void AudioMixerParameterValueSaved(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        LogInformation(id: 2, nameof(AudioMixerParameterValueSaved), $"Saved {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} to {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public void AudioMixerParameterPrefDeleted(string preferencesKey) =>
        LogInformation(id: 3, nameof(AudioMixerParameterPrefDeleted), $"Deleted {{{nameof(preferencesKey)}}}", preferencesKey);

    public void CurrentSafeArea(RectTransform rectTransform) =>
        LogInformation(id: 4, nameof(CurrentSafeArea),
            $"Current anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}}). " +
            $"Updating for current screen ({{screenWidth}} x {{screenHeight}}) and safe area ({{safeAreaWidth}} x {{safeAreaHeight}}).",
            rectTransform.GetHierarchyNameWithType(),
            rectTransform.anchorMin, rectTransform.anchorMax,
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height
        );

    public void NewSafeArea(RectTransform rectTransform) =>
        LogInformation(id: 5, nameof(NewSafeArea),
            $"New anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}})",
            rectTransform.GetHierarchyNameWithType(), rectTransform.anchorMin, rectTransform.anchorMax
        );

    public void UiBreakpointUpdating(BreakpointMode mode, BreakpointMatchMode matchMode) =>
        LogInformation(id: 6, nameof(UiBreakpointUpdating),
            $"Current screen dimensions are ({{screenWidth}} x {{screenHeight}}) (screen), ({{safeAreaWidth}} x {{safeAreaHeight}}) (safe area). " +
            $"Updating breakpoints with {{{nameof(mode)}}} and {{{nameof(matchMode)}}}...",
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height,
            mode, matchMode
        );

    public void SplashScreenInitializing() =>
        LogInformation(id: 7, nameof(SplashScreenInitializing), "Initializing splash screen...");

    public void SplashScreenDrawing() =>
        LogInformation(id: 8, nameof(SplashScreenDrawing), "Starting to draw splash screen...");

    public void SplashScreenStopping() =>
        LogInformation(id: 9, nameof(SplashScreenStopping), "Stopping splash screen...");

    #endregion

    #region Warning

    public void UiBreakpointNegativeModeValue() =>
        LogWarning(id: 0, nameof(UiBreakpointNegativeModeValue), "UI breakpoints can only be matched against non-negative values. You can ignore this warning if you just restarted the Editor.");

    #endregion

    #region Error

    public void UiStackPushNullTrigger() =>
        LogError(id: 0, nameof(UiStackPushNullTrigger), $"A trigger must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is popped later");

    #endregion
}
