using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Audio;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.UI;

/// <inheritdoc/>
internal class UiLogger<T> : BaseUnityUtilLogger<T>
{
    public UiLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 14_000) { }

    #region Information

    public void AudioMixerParameterFromPrefs(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        Log(id: 0, nameof(AudioMixerParameterFromPrefs), Information, $"Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} from {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public void AudioMixerParameterFromInspector(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        Log(id: 1, nameof(AudioMixerParameterFromInspector), Information, $"Not using local preferences or key {{{nameof(preferencesKey)}}} could not be found. Loading {{{nameof(value)}}} of {{{nameof(parameter)}}} directly from {{{nameof(audioMixer)}}} instead", preferencesKey, value, parameter, audioMixer);

    public void AudioMixerParameterValueSaved(float value, string parameter, AudioMixer audioMixer, string preferencesKey) =>
        Log(id: 2, nameof(AudioMixerParameterValueSaved), Information, $"Saved {{{nameof(value)}}} of {{{nameof(parameter)}}} of {{{nameof(audioMixer)}}} to {{{nameof(preferencesKey)}}}", value, parameter, audioMixer.name, preferencesKey);

    public void AudioMixerParameterPrefDeleted(string preferencesKey) =>
        Log(id: 3, nameof(AudioMixerParameterPrefDeleted), Information, $"Deleted {{{nameof(preferencesKey)}}}", preferencesKey);

    public void CurrentSafeArea(RectTransform rectTransform) =>
        Log(id: 4, nameof(CurrentSafeArea), Information,
            $"Current anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}}). " +
            $"Updating for current screen ({{screenWidth}} x {{screenHeight}}) and safe area ({{safeAreaWidth}} x {{safeAreaHeight}}).",
            rectTransform.GetHierarchyNameWithType(),
            rectTransform.anchorMin, rectTransform.anchorMax,
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height
        );

    public void NewSafeArea(RectTransform rectTransform) =>
        Log(id: 5, nameof(NewSafeArea), Information,
            $"New anchors of {{{nameof(rectTransform)}}}: ({{anchorMin}}, {{anchorMax}})",
            rectTransform.GetHierarchyNameWithType(), rectTransform.anchorMin, rectTransform.anchorMax
        );

    public void UiBreakpointUpdating(BreakpointMode mode, BreakpointMatchMode matchMode) =>
        Log(id: 6, nameof(UiBreakpointUpdating), Information,
            $"Current screen dimensions are ({{screenWidth}} x {{screenHeight}}) (screen), ({{safeAreaWidth}} x {{safeAreaHeight}}) (safe area). " +
            $"Updating breakpoints with {{{nameof(mode)}}} and {{{nameof(matchMode)}}}...",
            Screen.width, Screen.height,
            Screen.safeArea.width, Screen.safeArea.height
        );

    #endregion

    #region Warning

    public void UiBreakpointNegativeModeValue() =>
        Log(id: 0, nameof(UiBreakpointNegativeModeValue), Warning, "UI breakpoints can only be matched against non-negative values. You can ignore this warning if you just restarted the Editor.");

    #endregion

    #region Error

    public void UiStackPushNullTrigger() =>
        Log(id: 0, nameof(UiStackPushNullTrigger), Error, $"A trigger must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is popped later");

    #endregion
}
