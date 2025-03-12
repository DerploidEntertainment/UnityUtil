using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using UnityUtil.Storage;

namespace UnityUtil.UI;

public enum AudioSliderTransformation
{
    Linear,
    /// <summary>
    /// See <a href="https://gamedevbeginner.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/">this article</a> for explanation of why volume sliders need to use a logarithmic transformation.
    /// </summary>
    Logarithmic
}

[TypeInfoBox(
    $"Make sure this component is enabled (and its {nameof(GameObject)} is active) on startup, " +
    $"otherwise initialization of {nameof(AudioMixer)} from local preferences will not work correctly." +
    $"\n\nNote that an {nameof(EventTrigger)} component will be attached to {nameof(UnityEngine.UI.Slider)} (if one isn't attached already), " +
    $"so that we can listen for {nameof(EventTriggerType.PointerUp)} events to play {nameof(TestAudio)} and save to local preferences. " +
    $"This will make the {nameof(Slider)}'s {nameof(GameObject)} intercept all events, " +
    $"and no event bubbling will occur from that object!"
)]
public class AudioMixerParameterSlider : MonoBehaviour
{

    private UiLogger<AudioMixerParameterSlider>? _logger;
    private ILocalPreferences? _localPreferences;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public AudioMixer? AudioMixer;

    [Tooltip($"This parameter must already be exposed on {nameof(AudioMixer)}, and its value will be updated as the user updates {nameof(Slider)}.")]
    public string ExposedParameterName = "Volume";

    [Tooltip(
        $"The value of this {nameof(UnityEngine.UI.Slider)} will be used to update the exposed parameter of {nameof(AudioMixer)}. " +
        $"Its value will be transformed according to {nameof(SliderTransformation)}."
    )]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Slider? Slider;

    [Tooltip(
        $"Optional. This {nameof(AudioSource)} will be played anytime the {nameof(Slider)}'s value is changed, " +
        $"so that the user can hear the difference. Make sure that its output {nameof(AudioMixerGroup)} is set correctly. " +
        $"The {nameof(AudioClip)} played should not be long (like music), so as not to annoy the user."
    )]
    public AudioSource? TestAudio;

    [Tooltip(
        $"If true, then {nameof(Slider)}'s value (after transformation) will be saved to local preferences, " +
        $"so that it is 'saved' between sessions, and can theoretically be edited by the user."
    )]
    public bool StoreParameterInPreferences = true;

    [ShowIf(nameof(StoreParameterInPreferences))]
    [Tooltip($"If empty, the value of {nameof(ExposedParameterName)} will be used as key.")]
    public string PreferencesKey = "";


    [Header("Slider to Volume Conversion")]

    [Tooltip(
        $"How {nameof(Slider)}'s value is transformed to the new value of the exposed parameter of {nameof(AudioMixer)}." +
        $"\nIf {nameof(AudioSliderTransformation.Linear)}, then the parameter's new value will equal {nameof(Coefficient)} * ({nameof(Slider)} value). " +
        $"Usually, you will want a {nameof(Coefficient)} of 1. " +
        $"\nIf {nameof(AudioSliderTransformation.Logarithmic)}, then the parameter's new value will equal " +
        $"{nameof(Coefficient)} * Log (base {nameof(LogBase)}) of ({nameof(Slider)} value). " +
        $"\nWhen transforming volumes, you will want to use {nameof(AudioSliderTransformation.Logarithmic)} with a " +
        $"{nameof(LogBase)} of 10 and a {nameof(Coefficient)} of 20 " +
        $"(and your slider should have a {nameof(UnityEngine.UI.Slider.minValue)} and {nameof(UnityEngine.UI.Slider.maxValue)} of 0.0001 and 1, respectively)."
    )]
    public AudioSliderTransformation SliderTransformation = AudioSliderTransformation.Linear;

    [ShowIf(nameof(SliderTransformation), AudioSliderTransformation.Logarithmic)]
    [Tooltip($"See {nameof(SliderTransformation)} for the purpose of this field.")]
    public float LogBase = 10f;

    [Tooltip($"See {nameof(SliderTransformation)} for the purpose of this field.")]
    public float Coefficient = 1f;

    public string FinalPreferencesKey => string.IsNullOrEmpty(PreferencesKey) ? ExposedParameterName : PreferencesKey;

    public void Inject(ILoggerFactory loggerFactory, ILocalPreferences localPreferences)
    {
        _logger = new(loggerFactory, context: this);
        _localPreferences = localPreferences;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        bool paramExposed = AudioMixer!.GetFloat(ExposedParameterName, out _);
        if (!paramExposed)
            throw new InvalidOperationException($"{nameof(AudioMixer)} must expose a parameter with the name specified by {nameof(ExposedParameterName)} ('{ExposedParameterName}')");

        // Update AudioMixer and preferences (if requested) whenever slider changes
        Slider!.onValueChanged.AddListener(sliderValue => {
            float newVal = transformValue(sliderValue);
            _ = AudioMixer.SetFloat(ExposedParameterName, newVal);
        });

        // If a test audio was set, then listen for PointerUp events on the slider
        // Using the Slider's onValueChanged event leads to crazy rapid restarting of the test audio as user scrolls the Slider :P
        EventTrigger eventTrigger = Slider.GetComponent<EventTrigger>() ?? Slider.gameObject.AddComponent<EventTrigger>();
        var pointerUpEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEvent.callback.AddListener(e => {
            if (StoreParameterInPreferences) {
                float newVal = transformValue(Slider.value);
                _localPreferences!.SetFloat(FinalPreferencesKey, newVal);
                _logger!.AudioMixerParameterValueSaved(newVal, ExposedParameterName, AudioMixer, FinalPreferencesKey);
            }
#pragma warning disable IDE0031 // Use null propagation. C# doesn't allow overloading null-coalescing operators, so they don't work with Unity Objects' custom null logic...
            if (TestAudio != null)
                TestAudio.Play();
#pragma warning restore IDE0031 // Use null propagation
        });
        eventTrigger.triggers.Add(pointerUpEvent);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Start()
    {
        // Initialize audio parameters from preferences, if requested
        // This must occur in Start, as apparently setting AudioMixer parameters in Awake is undefined behavior... https://fogbugz.unity3d.com/default.asp?1197165_nik4gg1io942ae13#bugevent_1071843210
        float val;
        string prefsKey = FinalPreferencesKey;
        if (StoreParameterInPreferences && _localPreferences!.HasKey(prefsKey)) {
            val = _localPreferences.GetFloat(prefsKey);
            _logger!.AudioMixerParameterFromPrefs(val, ExposedParameterName, AudioMixer!, prefsKey);
        }
        else {
            _ = AudioMixer!.GetFloat(ExposedParameterName, out val);
            _logger!.AudioMixerParameterFromInspector(val, ExposedParameterName, AudioMixer!, prefsKey);
        }

        Slider!.value = untransformValue(val);   // This will trigger onValueChanged and thus initialize the AudioMixer as well
    }

    private float untransformValue(float transformedValue) =>
        SliderTransformation switch {
            AudioSliderTransformation.Linear => transformedValue / Coefficient,
            AudioSliderTransformation.Logarithmic => Mathf.Pow(LogBase, transformedValue / Coefficient),
            _ => throw UnityObjectExtensions.SwitchDefaultException(SliderTransformation)
        };
    private float transformValue(float sliderValue) =>
        SliderTransformation switch {
            AudioSliderTransformation.Linear => sliderValue * Coefficient,
            AudioSliderTransformation.Logarithmic => Mathf.Log(sliderValue, LogBase) * Coefficient,
            _ => throw UnityObjectExtensions.SwitchDefaultException(SliderTransformation)
        };

    [Button]
    public void ClearPreferences()
    {
        string prefsKey = FinalPreferencesKey;
        if (_localPreferences == null)
            PlayerPrefs.DeleteKey(prefsKey);
        else
            _localPreferences.DeleteKey(prefsKey);

        // Use debug logger in case this is being run from the Inspector outside Play mode
        _logger ??= new(new UnityDebugLoggerFactory(), context: this);
        _logger.AudioMixerParameterPrefDeleted(prefsKey);
    }

}
