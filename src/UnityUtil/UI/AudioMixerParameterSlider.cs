using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Logging;
using UnityEngine.Storage;

namespace UnityEngine.UI
{

    public enum AudioSliderTransformation {
        Linear,
        /// <summary>
        /// See <a href="https://gamedevbeginner.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/">this article</a> for explanation of why volume sliders need to use a logarithmic transformation.
        /// </summary>
        Logarithmic
    }

    [TypeInfoBox(
        $"Make sure this component is enabled (and its {nameof(GameObject)} is active) on startup, " +
        $"otherwise initialization of {nameof(AudioMixer)} from the cache will not work correctly." +
        $"\n\nNote that an {nameof(EventTrigger)} component will be attached to {nameof(UI.Slider)} (if one isn't attached already), " +
        $"so that we can listen for {nameof(EventTriggerType.PointerUp)} events to play {nameof(TestAudio)} and save to a cache. " +
        $"This will make the {nameof(Slider)}'s {nameof(GameObject)} intercept all events, " +
        $"and no event bubbling will occur from that object!"
    )]
    public class AudioMixerParameterSlider : Configurable {

        private ILogger? _logger;
        private ILocalCache? _localCache;

        [Required]
        public AudioMixer? AudioMixer;

        [Tooltip($"This parameter must already be exposed on {nameof(AudioMixer)}, and its value will be updated as the user updates {nameof(Slider)}.")]
        public string ExposedParameterName = "Volume";

        [Tooltip(
            $"The value of this {nameof(UI.Slider)} will be used to update the exposed parameter of {nameof(AudioMixer)}. " +
            $"Its value will be transformed according to {nameof(SliderTransformation)}."
        )]
        [Required]
        public Slider? Slider;

        [Tooltip(
            $"Optional. This {nameof(AudioSource)} will be played anytime the {nameof(Slider)}'s value is changed, " +
            $"so that the user can hear the difference. Make sure that its output {nameof(AudioMixerGroup)} is set correctly. " +
            $"The {nameof(AudioClip)} played should not be long (like music), so as not to annoy the user."
        )]
        public AudioSource? TestAudio;

        [Tooltip(
            $"If true, then {nameof(Slider)}'s value (after transformation) will be saved to a cache, " +
            $"so that it is 'saved' between sessions, and can theoretically be edited by the user."
        )]
        public bool StoreParameterInCache = true;

        [ShowIf(nameof(StoreParameterInCache))]
        [Tooltip($"If empty, the value of {nameof(ExposedParameterName)} will be used as key.")]
        public string CacheKey = "";


        [Header("Slider to Volume Conversion")]

        [Tooltip(
            $"How {nameof(Slider)}'s value is transformed to the new value of the exposed parameter of {nameof(AudioMixer)}." +
            $"\nIf {nameof(AudioSliderTransformation.Linear)}, then the parameter's new value will equal {nameof(Coefficient)} * ({nameof(Slider)} value). " +
            $"Usually, you will want a {nameof(Coefficient)} of 1. " +
            $"\nIf {nameof(AudioSliderTransformation.Logarithmic)}, then the parameter's new value will equal " +
            $"{nameof(Coefficient)} * Log (base {nameof(LogBase)}) of ({nameof(Slider)} value). " +
            $"\nWhen transforming volumes, you will want to use {nameof(AudioSliderTransformation.Logarithmic)} with a " +
            $"{nameof(LogBase)} of 10 and a {nameof(Coefficient)} of 20 " +
            $"(and your slider should have a {nameof(UI.Slider.minValue)} and {nameof(UI.Slider.maxValue)} of 0.0001 and 1, respectively)."
        )]
        public AudioSliderTransformation SliderTransformation = AudioSliderTransformation.Linear;

        [ShowIf(nameof(SliderTransformation), AudioSliderTransformation.Logarithmic)]
        [Tooltip($"See {nameof(SliderTransformation)} for the purpose of this field.")]
        public float LogBase = 10f;

        [Tooltip($"See {nameof(SliderTransformation)} for the purpose of this field.")]
        public float Coefficient = 1f;

        public string FinalCacheKey => string.IsNullOrEmpty(CacheKey) ? ExposedParameterName : CacheKey;

        public void Inject(ILoggerProvider loggerProvider, ILocalCache localCache) {
            _logger = loggerProvider.GetLogger(this);
            _localCache = localCache;
        }
        protected override void Awake() {
            base.Awake();

            bool paramExposed = AudioMixer!.GetFloat(ExposedParameterName, out _);
            Assert.IsTrue(paramExposed, $"{nameof(AudioMixer)} must expose a parameter with the name specified by {nameof(ExposedParameterName)} ('{ExposedParameterName}')");

            // Update AudioMixer and cache (if requested) whenever slider changes
            Slider!.onValueChanged.AddListener(sliderValue => {
                float newVal = transformValue(sliderValue);
                AudioMixer.SetFloat(ExposedParameterName, newVal);
            });

            // If a test audio was set, then listen for PointerUp events on the slider
            // Using the Slider's onValueChanged event leads to crazy rapid restarting of the test audio as user scrolls the Slider :P
            EventTrigger eventTrigger = Slider.GetComponent<EventTrigger>() ?? Slider.gameObject.AddComponent<EventTrigger>();
            var pointerUpEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEvent.callback.AddListener(e => {
                if (StoreParameterInCache) {
                    float newVal = transformValue(Slider.value);
                    _localCache!.SetFloat(FinalCacheKey, newVal);
                    _logger!.Log($"Saved new value ({newVal}) of exposed parameter '{ExposedParameterName}' of {nameof(Audio.AudioMixer)} '{AudioMixer.name}' to cache", context: this);
                }
                if (TestAudio is not null)
                    TestAudio.Play();   // Don't know why the F*CK a null-coalescing operator isn't working here...
            });
            eventTrigger.triggers.Add(pointerUpEvent);
        }
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Start() {
            // Initialize audio parameters from cache, if requested
            // This must occur in Start, as apparently setting AudioMixer parameters in Awake is undefined behavior... https://fogbugz.unity3d.com/default.asp?1197165_nik4gg1io942ae13#bugevent_1071843210
            float val;
            string logMsg;

            string cacheKey = FinalCacheKey;
            if (StoreParameterInCache && _localCache!.HasKey(cacheKey)) {
                val = _localCache.GetFloat(cacheKey);
                logMsg = $"Loaded value ({val}) of exposed parameter '{ExposedParameterName}' of {nameof(Audio.AudioMixer)} '{AudioMixer!.name}' from cache";
            }
            else {
                AudioMixer!.GetFloat(ExposedParameterName, out val);
                logMsg = $"Not using cache or key '{cacheKey}' could not be found. Loaded value of exposed parameter '{ExposedParameterName}' from {nameof(Audio.AudioMixer)} '{AudioMixer.name}' instead";
            }

            Slider!.value = untransformValue(val);   // This will trigger onValueChanged and thus initialize the AudioMixer as well
            _logger!.Log(logMsg, context: this);
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
        public void ClearCachedState() {
            string cacheKey = FinalCacheKey;
            if (_localCache is null)
                PlayerPrefs.DeleteKey(cacheKey);
            else
                _localCache.DeleteKey(cacheKey);

            // Use debug logger in case this is being run from the Inspector outside Play mode
            _logger ??= Debug.unityLogger;
            Debug.Log($"Deleted cache key '{cacheKey}'.", context: this);
        }

    }

}
