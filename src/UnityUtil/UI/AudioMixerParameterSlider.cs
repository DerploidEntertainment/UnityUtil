using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Logging;
using UnityEngine.UI;
using UI = UnityEngine.UI;

namespace UnityEngine.UI {

    public enum AudioSliderTransformation {
        Linear,
        /// <summary>
        /// See <a href="https://gamedevbeginner.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/">this article</a> for explanation of why volume sliders need to use a logarithmic transformation.
        /// </summary>
        Logarithmic
    }

    [TypeInfoBox("Make sure this component is enabled (and its " + nameof(GameObject) + " is active) on startup, otherwise initialization of " + nameof(AudioMixer) + " from " + nameof(PlayerPrefs) + " will not work correctly.\n\nNote that an " + nameof(EventTrigger) + " component will be attached to " + nameof(UI.Slider) + " (if one isn't attached already), so that we can listen for " + nameof(EventTriggerType.PointerUp) + " events to play " + nameof(TestAudio) + " and save " + nameof(PlayerPrefs) + ". This will make the " + nameof(Slider) + "'s " + nameof(GameObject) + " intercept all events, and no event bubbling will occur from that object!")]
    public class AudioMixerParameterSlider : Configurable {

        private ILogger _logger;

        public AudioMixer AudioMixer;
        [Tooltip("This parameter must already be exposed on " + nameof(AudioMixer) + ", and its value will be updated as the user updates " + nameof(Slider) + ".")]
        public string ExposedParameterName;
        [Tooltip("The value of this " + nameof(UI.Slider) + " will be used to update the exposed parameter of " + nameof(AudioMixer) + ". Its value will be transformed according to " + nameof(SliderTransformation) + ".")]
        public Slider Slider;
        [Tooltip("Optional. This " + nameof(AudioSource) + " will be played anytime the " + nameof(Slider) + "'s value is changed, so that the user can hear the difference. Make sure that its output " + nameof(AudioMixerGroup) + " is set correctly. The " + nameof(AudioClip) + " played should not be long (like music), so as not to annoy the user.")]
        public AudioSource TestAudio;
        [Tooltip("If true, then " + nameof(Slider) + "'s value (after transformation) will be saved to " + nameof(PlayerPrefs) + ", so that it is 'saved' between sessions, and can theoretically be edited by the user.")]
        public bool StoreParameterInPlayerPrefs;
        [ShowIf(nameof(StoreParameterInPlayerPrefs))]
        [Tooltip("If empty, the value of " + nameof(ExposedParameterName) + " will be used as key.")]
        public string PlayerPrefKey;
        [Tooltip("How " + nameof(Slider) + "'s value is transformed to the new value of the exposed parameter of " + nameof(AudioMixer) + ".\nIf " + nameof(AudioSliderTransformation.Linear) + ", then the parameter's new value will equal " + nameof(Coefficient) + " * (" + nameof(Slider) + " value).\nIf " + nameof(AudioSliderTransformation.Logarithmic) + ", then the parameter's new value will equal " + nameof(Coefficient) + " * Log (base " + nameof(LogBase) + ") of (" + nameof(Slider) + " value). Usually, for " + nameof(AudioSliderTransformation.Linear) + " transformations, you will want a " + nameof(Coefficient) + " of 1. When transforming volumes, you will want to use " + nameof(AudioSliderTransformation.Logarithmic) + " with a " + nameof(LogBase) + " of 10 and a " + nameof(Coefficient) + " of 20 (and your slider should have a " + nameof(UI.Slider.minValue) + " and " + nameof(UI.Slider.maxValue) + " of 0.0001 and 1, respectively).")]
        public AudioSliderTransformation SliderTransformation;
        [ShowIf(nameof(SliderTransformation), AudioSliderTransformation.Logarithmic)]
        [Tooltip("See " + nameof(SliderTransformation) + " for the purpose of this field.")]
        public float LogBase;
        [Tooltip("See " + nameof(SliderTransformation) + " for the purpose of this field.")]
        public float Coefficient;

        public string FinalPlayerPrefKey => string.IsNullOrEmpty(PlayerPrefKey) ? ExposedParameterName : PlayerPrefKey;

        private void Reset() {
            ExposedParameterName = "Volume";

            StoreParameterInPlayerPrefs = true;
            PlayerPrefKey = "";

            SliderTransformation = AudioSliderTransformation.Linear;
            LogBase = 10f;
            Coefficient = 1f;
        }
        public void Inject(ILoggerProvider loggerProvider) {
            _logger = loggerProvider.GetLogger(this);
        }
        protected override void OnAwake() {
            base.OnAwake();

            this.AssertAssociation(AudioMixer, nameof(AudioMixer));
            this.AssertAssociation(Slider, nameof(Slider));

            bool paramExposed = AudioMixer.GetFloat(ExposedParameterName, out _);
            Assert.IsTrue(paramExposed, $"{nameof(AudioMixer)} must expose a parameter with the name specified by {nameof(ExposedParameterName)} ('{ExposedParameterName}')");

            // Update AudioMixer and PlayerPrefs (if requested) whenever slider changes
            Slider.onValueChanged.AddListener(sliderValue => {
                float newVal = transformValue(sliderValue);
                AudioMixer.SetFloat(ExposedParameterName, newVal);
            });

            // If a test audio was set, then listen for PointerUp events on the slider
            // Using the Slider's onValueChanged event leads to crazy rapid restarting of the test audio as user scrolls the Slider :P
            EventTrigger eventTrigger = Slider.GetComponent<EventTrigger>() ?? Slider.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entry.callback.AddListener(e => {
                if (StoreParameterInPlayerPrefs) {
                    float newVal = transformValue(Slider.value);
                    PlayerPrefs.SetFloat(FinalPlayerPrefKey, newVal);
                    _logger.Log($"Saved new value ({newVal}) of exposed parameter '{ExposedParameterName}' of {nameof(Audio.AudioMixer)} '{AudioMixer.name}' to {nameof(PlayerPrefs)}", context: this);
                }
                if (TestAudio != null)
                    TestAudio.Play();   // Don't know why the F*CK a null-coalescing operator isn't working here...
            });
            eventTrigger.triggers.Add(entry);
        }
        private void Start() {
            // Initialize audio parameters from PlayerPrefs, if requested
            // This must occur in Start, as apparently setting AudioMixer parameters in Awake is undefined behavior... https://fogbugz.unity3d.com/default.asp?1197165_nik4gg1io942ae13#bugevent_1071843210
            float val;
            string logMsg;

            string playerPrefKey = FinalPlayerPrefKey;
            if (StoreParameterInPlayerPrefs && PlayerPrefs.HasKey(playerPrefKey)) {
                val = PlayerPrefs.GetFloat(playerPrefKey);
                logMsg = $"Loaded value ({val}) of exposed parameter '{ExposedParameterName}' of {nameof(Audio.AudioMixer)} '{AudioMixer.name}' from {nameof(PlayerPrefs)}";
            }
            else {
                AudioMixer.GetFloat(ExposedParameterName, out val);
                logMsg = $"Not using {nameof(PlayerPrefs)} or key '{playerPrefKey}' could not be found. Loaded value of exposed parameter '{ExposedParameterName}' from {nameof(Audio.AudioMixer)} '{AudioMixer.name}' instead";
            }

            Slider.value = untransformValue(val);   // This will trigger onValueChanged and thus initialize the AudioMixer as well
            _logger.Log(logMsg, context: this);
        }

        private float untransformValue(float transformedValue) =>
            SliderTransformation switch {
                AudioSliderTransformation.Linear => transformedValue / Coefficient,
                AudioSliderTransformation.Logarithmic => Mathf.Pow(LogBase, transformedValue / Coefficient),
                _ => throw new NotImplementedException(UnityObjectExtensions.GetSwitchDefault(SliderTransformation))
            };
        private float transformValue(float sliderValue) =>
            SliderTransformation switch {
                AudioSliderTransformation.Linear => sliderValue * Coefficient,
                AudioSliderTransformation.Logarithmic => Mathf.Log(sliderValue, LogBase) * Coefficient,
                _ => throw new NotImplementedException(UnityObjectExtensions.GetSwitchDefault(SliderTransformation))
            };

    }

}
