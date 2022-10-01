using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityUtil.DependencyInjection;

namespace UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]   // So the OnRectTransformDimensionsChange message gets called
[ExecuteAlways]                             // So the OnRectTransformDimensionsChange message gets called in the Editor too...kind of a necessity for UI tweaking
[TypeInfoBox(
    "Note that changes to some fields may not take effect until the next time the UI value is updated " +
    "(e.g., by changing the size of the Game window or the frustum of the Camera).\n\n" +
    "Also, updates may take a few seconds if the attached RectTransform is deeply nested in the hierarchy."
)]
public class UiBreakpoints : MonoBehaviour
{
    private ILogger? _logger;

    private bool _noMatch;

    [ShowInInspector, ReadOnly, Tooltip("Width x Height")]
    private Vector2 _currentDimensions;

    [ShowInInspector, ReadOnly]
    private float _currentValue;

    [Tooltip(
        "What value will breakpoints be matched against? Can be the width, height, or aspect ratio of the physical device screen, " +
        "the device's 'safe area', or a particular Camera."
    )]
    public BreakpointMode Mode = BreakpointMode.SafeAreaAspectRatio;

    [Tooltip(
        $"How will breakpoints be matched against the value specified by {nameof(Mode)}? " +
        "You can use this to specify UI that applies at only a specific value, or across a range of values. " +
        $"For example, suppose that {nameof(Mode)} is {nameof(BreakpointMode.ScreenWidth)}, " +
        "and you provide breakpoints at values of 576, 768, and 1200 on a screen that is 700 pixels wide. Then:" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(BreakpointMatchMode.AnyEqualOrGreater)}, then the 768 and 1200 breakpoints will match and have their {nameof(UiBreakpoint.Matched)} event raised" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(BreakpointMatchMode.AnyEqualOrLess)}, then only the 576 breakpoint will match" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(BreakpointMatchMode.MaxEqualOrLess)}, then only the 576 breakpoint will match (or the 768 breakpoint on a device that's exactly 768 pixels wide)" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(BreakpointMatchMode.MinEqualOrGreater)}, then only the 768 breakpoint will match"
    )]
    public BreakpointMatchMode MatchMode = BreakpointMatchMode.MinEqualOrGreater;

    [ShowIf(nameof(IsCameraMode))]
    [Tooltip("This is the Camera whose height, width, or aspect ratio will be matched against the provided breakpoints.")]
    public Camera? Camera;

    [Tooltip(
        "If true, then breakpoint matches will be checked again every time the game's window is resized. " +
        "This allows UI to adjust dynamically to changing window sizes in standalone players, or at design time in the Editor Game window. " +
        "If false, then breakpoints will not be matched until entering play mode. " +
        "Note that, if this value is true, then handlers for the breakpoint match events will run in the Editor " +
        "if they are set to run in 'Editor and Runtime', and then they will run even if this component is disabled."
    )]
    public bool RecheckMatchesOnResize = true;

    [Tooltip("Should the dimensions of the screen and safe area be logged in the Editor? Useful for debugging, but makes the Console quite noisy.")]
    public bool LogDimensionsInEditor = true;

    [Tooltip("Should the dimensions of the screen and safe area be logged in a built player? Useful for troubleshooting in Development builds.")]
    public bool LogDimensionsInPlayer = false;

    [InfoBox($"No matching breakpoints. Raising {nameof(NoBreakpointMatched)} event instead", nameof(_noMatch))]
    [TableList(AlwaysExpanded = true), ValidateInput(nameof(AreBreakpointsValid), "Breakpoint values must be provided in ascending order with no duplicates")]
    public UiBreakpoint[] Breakpoints = Array.Empty<UiBreakpoint>();

    [Tooltip(
        "If no breakpoints are matched, then this UnityEvent will be raised instead, e.g., " +
        "to set any UI defaults that are not already set in the Inspector. " +
        $"For example, when breakpoints are being matched in {nameof(Mode)} '{nameof(BreakpointMode.ScreenWidth)}' " +
        $"and {nameof(MatchMode)} '{nameof(BreakpointMatchMode.AnyEqualOrLess)}' against a 700px-wide device, " +
        $"but the only breakpoint provided is for a value of 1200, then no breakpoint {nameof(UiBreakpoint.Matched)} events " +
        "will be raised, so this event will be raised instead."
    )]
    public UnityEvent NoBreakpointMatched = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake()
    {
        if (DependencyInjector.Instance.Initialized)
            DependencyInjector.Instance.ResolveDependenciesOf(this);
    }

    public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Start()
    {
        // Using Start rather than Awake cause we don't want to mess with breakpoints being raised by Awake during Edit Mode tests
        // Awake is called by AddComponent since we've added the ExecuteAlwaysAttribute to this class

        _currentValue = getModeValue(Mode);
        InvokeMatchingBreakpoints(_currentValue);
    }

    /// <summary>
    /// This Unity message is not documented in the MonoBehaviour docs, but apparently it IS a message that any MonoBehaviour can receive (not just UIBehaviour)
    /// See this <a href="https://www.programmersought.com/article/1195140410/">weird and obscure source</a> :P
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void OnRectTransformDimensionsChange()
    {
        if (!RecheckMatchesOnResize)
            return;

        if (Device.Application.isEditor) {
            _currentDimensions =
                IsScreenMode ? new Vector2(Device.Screen.width, Device.Screen.height) :
                IsSafeAreaMode ? new Vector2(Device.Screen.safeArea.width, Device.Screen.safeArea.height) :
                Camera != null ? new Vector2(Camera.pixelWidth, Camera.pixelHeight) :
                Vector2.zero;
        }
        _currentValue = getModeValue(Mode);

        InvokeMatchingBreakpoints(_currentValue);
    }

    public bool IsScreenMode => Mode == BreakpointMode.ScreenWidth || Mode == BreakpointMode.ScreenHeight || Mode == BreakpointMode.ScreenAspectRatio;
    public bool IsSafeAreaMode => Mode == BreakpointMode.SafeAreaWidth || Mode == BreakpointMode.SafeAreaHeight || Mode == BreakpointMode.SafeAreaAspectRatio;
    public bool IsCameraMode => Mode == BreakpointMode.CameraWidth || Mode == BreakpointMode.CameraHeight || Mode == BreakpointMode.CameraAspectRatio;
    public bool IsWidthMode => Mode == BreakpointMode.ScreenWidth || Mode == BreakpointMode.SafeAreaWidth || Mode == BreakpointMode.CameraWidth;
    public bool IsHeightMode => Mode == BreakpointMode.ScreenHeight || Mode == BreakpointMode.SafeAreaHeight || Mode == BreakpointMode.CameraHeight;
    public bool IsAspectRatioMode => Mode == BreakpointMode.ScreenAspectRatio || Mode == BreakpointMode.SafeAreaAspectRatio || Mode == BreakpointMode.CameraAspectRatio;

    internal static bool AreBreakpointsValid(UiBreakpoint[] breakpoints)
    {
        if (breakpoints.Length <= 1)
            return true;

        // Make sure breakpoint values are sorted strictly ascending (no duplicates)
        for (int b = 1; b < breakpoints.Length; ++b) {
            if (
                breakpoints[b].Enabled && breakpoints[b - 1].Enabled &&
                breakpoints[b].Value <= breakpoints[b - 1].Value
            )
                return false;
        }

        return true;
    }

    private float getModeValue(BreakpointMode mode)
    {
        return mode switch {
            BreakpointMode.ScreenWidth => Device.Screen.width,
            BreakpointMode.ScreenHeight => Device.Screen.height,
            BreakpointMode.ScreenAspectRatio => (float)Device.Screen.width / Device.Screen.height,

            BreakpointMode.SafeAreaWidth => Device.Screen.safeArea.width,
            BreakpointMode.SafeAreaHeight => Device.Screen.safeArea.height,
            BreakpointMode.SafeAreaAspectRatio => Device.Screen.safeArea.width / Device.Screen.safeArea.height,

            BreakpointMode.CameraWidth => Camera?.pixelWidth ?? throw getNullCameraException(),
            BreakpointMode.CameraHeight => Camera?.pixelHeight ?? throw getNullCameraException(),
            BreakpointMode.CameraAspectRatio => Camera?.aspect ?? throw getNullCameraException(),

            _ => throw UnityObjectExtensions.SwitchDefaultException(Mode),
        };

        static Exception getNullCameraException() =>
            new InvalidOperationException(
                $"{nameof(Camera)} must be provided when {nameof(Mode)} is " +
                $"{nameof(BreakpointMode.CameraWidth)}, {nameof(BreakpointMode.CameraHeight)}, or {nameof(BreakpointMode.CameraAspectRatio)}."
            );
    }

    internal const string MsgNegativeModeValue = "UI breakpoints can only be matched against non-negative values. You can ignore this warning if you just restarted the Editor.";

    internal void InvokeMatchingBreakpoints(float modeValue)
    {
        _logger ??= Debug.unityLogger;

        if (modeValue < 0f) {
            _logger.LogWarning(MsgNegativeModeValue);
            return;
        }

        // Early exit if no breakpoints were provided
        if (Breakpoints.Length == 0)
            return;

        bool log = (Device.Application.isEditor && LogDimensionsInEditor) || (!Device.Application.isEditor && LogDimensionsInPlayer);
        if (log) {
            _logger.Log(
                $"Current screen dimensions (width x height) are {Device.Screen.width} x {Device.Screen.height} (screen) and {Device.Screen.safeArea.width} x {Device.Screen.safeArea.height} (safe area). " +
                $"Updating breakpoints with {nameof(Mode)} {Mode} and {nameof(MatchMode)} {MatchMode}..."
            , context: this);
        }

        // Reset all UI breakpoints to not-matched state
        _noMatch = true;
        for (int b = 0; b < Breakpoints.Length; ++b)
            Breakpoints[b].IsMatched = false;

        // Raise the "matched" event on all matching breakpoints, according to match criteria...
        if (MatchMode == BreakpointMatchMode.MaxEqualOrLess) {
            UiBreakpoint breakpoint = Breakpoints.LastOrDefault(b => b.Enabled && b.Value <= modeValue);
            if (breakpoint is not null)
                invokeBreakpoint(breakpoint);
        }
        else {
            for (int b = 0; b < Breakpoints.Length; ++b) {
                UiBreakpoint breakpoint = Breakpoints[b];
                if (!breakpoint.Enabled)
                    continue;

                bool invoke =
                    (breakpoint.Value < modeValue && MatchMode == BreakpointMatchMode.AnyEqualOrLess)
                    || breakpoint.Value == modeValue
                    || (breakpoint.Value > modeValue && (MatchMode == BreakpointMatchMode.AnyEqualOrGreater || MatchMode == BreakpointMatchMode.MinEqualOrGreater));
                bool earlyBreak = (breakpoint.Value >= modeValue && MatchMode == BreakpointMatchMode.MinEqualOrGreater);

                if (invoke)
                    invokeBreakpoint(breakpoint);
                if (earlyBreak)
                    break;
            }
        }

        // If no breakpoints matched the given criteria, then raise a "no match" event
        if (_noMatch)
            NoBreakpointMatched.Invoke();


        void invokeBreakpoint(UiBreakpoint breakpoint)
        {
            breakpoint.IsMatched = true;
            breakpoint.Matched.Invoke();
            _noMatch = false;
        }
    }

}
