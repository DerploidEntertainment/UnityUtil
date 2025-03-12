using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;
using static UnityUtil.UI.BreakpointMatchMode;
using static UnityUtil.UI.BreakpointMode;

namespace UnityUtil.UI;

[RequireComponent(typeof(RectTransform))]   // So the OnRectTransformDimensionsChange message gets called
[ExecuteAlways]                             // So the OnRectTransformDimensionsChange message gets called in the Editor too...kind of a necessity for UI tweaking
[TypeInfoBox(
    "Note that changes to some fields may not take effect until the next time the UI value is updated " +
    "(e.g., by changing the size of the Game window or the frustum of the Camera).\n\n" +
    "Also, updates may take a few seconds if the attached RectTransform is deeply nested in the hierarchy."
)]
public class UiBreakpoints : MonoBehaviour
{
    private UiLogger<UiBreakpoints>? _logger;

    private bool _noMatch;

    [ShowInInspector, ReadOnly, Tooltip("Width x Height")]
    private Vector2 _currentDimensions;

    [ShowInInspector, ReadOnly]
    private float _currentValue;

    [Tooltip(
        "What value will breakpoints be matched against? Can be the width, height, or aspect ratio of the physical device screen, " +
        "the device's 'safe area', or a particular Camera."
    )]
    public BreakpointMode Mode = SafeAreaAspectRatio;

    [Tooltip(
        $"How will breakpoints be matched against the value specified by {nameof(Mode)}? " +
        "You can use this to specify UI that applies at only a specific value, or across a range of values. " +
        $"For example, suppose that {nameof(Mode)} is {nameof(ScreenWidth)}, " +
        "and you provide breakpoints at values of 576, 768, and 1200 on a screen that is 700 pixels wide. Then:" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(AnyEqualOrGreater)}, then the 768 and 1200 breakpoints will match and have their {nameof(UiBreakpoint.Matched)} event raised" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(AnyEqualOrLess)}, then only the 576 breakpoint will match" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(MaxEqualOrLess)}, then only the 576 breakpoint will match (or the 768 breakpoint on a device that's exactly 768 pixels wide)" +
        $"\n\t- If {nameof(MatchMode)} is {nameof(MinEqualOrGreater)}, then only the 768 breakpoint will match"
    )]
    public BreakpointMatchMode MatchMode = MinEqualOrGreater;

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
    public UiBreakpoint[] Breakpoints = [];

    [Tooltip(
        "If no breakpoints are matched, then this UnityEvent will be raised instead, e.g., " +
        "to set any UI defaults that are not already set in the Inspector. " +
        $"For example, when breakpoints are being matched in {nameof(Mode)} '{nameof(ScreenWidth)}' " +
        $"and {nameof(MatchMode)} '{nameof(AnyEqualOrLess)}' against a 700px-wide device, " +
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

    public void Inject(ILoggerFactory loggerFactory) => _logger = new(loggerFactory, context: this);

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

        if (Application.isEditor) _currentDimensions =
            IsScreenMode ? new Vector2(Screen.width, Screen.height) :
            IsSafeAreaMode ? new Vector2(Screen.safeArea.width, Screen.safeArea.height) :
            Camera != null ? new Vector2(Camera.pixelWidth, Camera.pixelHeight) :
            Vector2.zero;
        _currentValue = getModeValue(Mode);

        InvokeMatchingBreakpoints(_currentValue);
    }

    public bool IsScreenMode => Mode is ScreenWidth or ScreenHeight or ScreenAspectRatio;
    public bool IsSafeAreaMode => Mode is SafeAreaWidth or SafeAreaHeight or SafeAreaAspectRatio;
    public bool IsCameraMode => Mode is CameraWidth or CameraHeight or CameraAspectRatio;
    public bool IsWidthMode => Mode is ScreenWidth or SafeAreaWidth or CameraWidth;
    public bool IsHeightMode => Mode is ScreenHeight or SafeAreaHeight or CameraHeight;
    public bool IsAspectRatioMode => Mode is ScreenAspectRatio or SafeAreaAspectRatio or CameraAspectRatio;

    internal static bool AreBreakpointsValid(UiBreakpoint[] breakpoints)
    {
        if (breakpoints.Length <= 1)
            return true;

        // Make sure breakpoint values are sorted strictly ascending (no duplicates)
        for (int b = 1; b < breakpoints.Length; ++b) 
            if (
                breakpoints[b].Enabled && breakpoints[b - 1].Enabled &&
                breakpoints[b].Value <= breakpoints[b - 1].Value
            )
                return false;

        return true;
    }

    private float getModeValue(BreakpointMode mode)
    {
        return mode switch {
            ScreenWidth => Screen.width,
            ScreenHeight => Screen.height,
            ScreenAspectRatio => (float)Screen.width / Screen.height,

            SafeAreaWidth => Screen.safeArea.width,
            SafeAreaHeight => Screen.safeArea.height,
            SafeAreaAspectRatio => Screen.safeArea.width / Screen.safeArea.height,

            CameraWidth => Camera?.pixelWidth ?? throw getNullCameraException(),
            CameraHeight => Camera?.pixelHeight ?? throw getNullCameraException(),
            CameraAspectRatio => Camera?.aspect ?? throw getNullCameraException(),

            _ => throw UnityObjectExtensions.SwitchDefaultException(Mode),
        };

        static Exception getNullCameraException() =>
            new InvalidOperationException(
                $"{nameof(Camera)} must be provided when {nameof(Mode)} is " +
                $"{nameof(CameraWidth)}, {nameof(CameraHeight)}, or {nameof(CameraAspectRatio)}."
            );
    }

    internal void InvokeMatchingBreakpoints(float modeValue)
    {
        _logger ??= new(new UnityDebugLoggerFactory(), context: this);

        if (modeValue < 0f) {
            _logger.UiBreakpointNegativeModeValue();
            return;
        }

        // Early exit if no breakpoints were provided
        if (Breakpoints.Length == 0)
            return;

        bool shouldLog = (Application.isEditor && LogDimensionsInEditor) || (!Application.isEditor && LogDimensionsInPlayer);
        if (shouldLog)
            _logger.UiBreakpointUpdating(Mode, MatchMode);

        // Reset all UI breakpoints to not-matched state
        _noMatch = true;
        for (int b = 0; b < Breakpoints.Length; ++b)
            Breakpoints[b].IsMatched = false;

        // Raise the "matched" event on all matching breakpoints, according to match criteria...
        if (MatchMode == MaxEqualOrLess) {
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
                    (breakpoint.Value < modeValue && MatchMode == AnyEqualOrLess)
                    || breakpoint.Value == modeValue
                    || (breakpoint.Value > modeValue && (MatchMode == AnyEqualOrGreater || MatchMode == MinEqualOrGreater));
                bool earlyBreak = breakpoint.Value >= modeValue && MatchMode == MinEqualOrGreater;

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
