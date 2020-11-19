using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Logging;

namespace UnityEngine.UI
{

    [RequireComponent(typeof(RectTransform))]   // So the OnRectTransformDimensionsChange message gets called
    [ExecuteAlways]                             // So the OnRectTransformDimensionsChange message gets called in the Editor too...kind of a necessity for UI tweaking
    [TypeInfoBox(
        "Note that changes to some fields may not take effect until the next time the UI value is updated " +
        "(e.g., by changing the size of the Game window or the frustum of the Camera).\n\n" +
        "Also, updates may take a few seconds if the attached RectTransform is deeply nested in the hierarchy."
    )]
    public class UiBreakpoints : UIBehaviour {

        private bool _noMatch;
        private float _currentValue;

        [Tooltip(
            "What value will breakpoints be matched against? Can be the width, height, or aspect ratio of the physical device screen, " +
            "the device's 'safe area', or a particular Camera."
        )]
        public BreakpointMode Mode;

        [ShowInInspector, ReadOnly]
        [Tooltip(
            "How will breakpoints be matched against the value specified by " + nameof(Mode) + "? " +
            "You can use this to specify UI that applies at only a specific value, or across a range of values. " +
            "For example, suppose that " + nameof(Mode) + " is " + nameof(BreakpointMode.ScreenWidth) + ", " +
            "and you provide breakpoints at values of 576, 768, and 1200 on a screen that is 700 pixels wide. " +
            "If " + nameof(MatchMode) + " is " + nameof(BreakpointMatchMode.AnyEqualOrGreater) + ", then the 768 and 1200 breakpoints will match and have their " + nameof(UiBreakpoint.Matched) + " event raised; " +
            "if " + nameof(MatchMode) + " is " + nameof(BreakpointMatchMode.AnyEqualOrLess) + ", then only the 576 breakpoint will match; " +
            "if " + nameof(MatchMode) + " is " + nameof(BreakpointMatchMode.MaxEqualOrLess) + ", then only the 576 breakpoint will match (or the 768 breakpoint on a device that's exactly 768 pixels wide); and " +
            "if " + nameof(MatchMode) + " is " + nameof(BreakpointMatchMode.MinEqualOrGreater) + ", then only the 768 breakpoint will match."
            )]
        public BreakpointMatchMode MatchMode;

        [ShowIf(nameof(IsCameraMode))]
        [Tooltip("This is the Camera whose height, width, or aspect ratio will be matched against the provided breakpoints.")]
        public Camera Camera;

        [Tooltip(
            "If true, then breakpoint matches will be checked again every time the game's window is resized. " +
            "This allows UI to adjust dynamically to changing window sizes in standalone players, or at design time in the Editor Game window. " +
            "If false, then breakpoints will not be matched until entering play mode. " +
            "Note that, if this value is true, then handlers for the breakpoint match events will run in the Editor " +
            "if they are set to run in 'Editor and Runtime', and then they will run even if this component is disabled."
        )]
        public bool RecheckMatchesOnResize;

        [InfoBox("No matching breakpoints. Raising " + nameof(NoBreakpointMatched) + " event instead", nameof(_noMatch))]
        [TableList(AlwaysExpanded = true), ValidateInput(nameof(AreBreakpointsValid), "Breakpoint values must be provided in ascending order with no duplicates")]
        public UiBreakpoint[] Breakpoints = Array.Empty<UiBreakpoint>();

        [Tooltip(
            "If no breakpoints are matched, then this UnityEvent will be raised instead, e.g., " +
            "to set any UI defaults that are not already set in the Inspector. " +
            "For example, when breakpoints are being matched in " + nameof(Mode) + " '" + nameof(BreakpointMode.ScreenWidth) +
            "' and " + nameof(MatchMode) + " '" + nameof(BreakpointMatchMode.AnyEqualOrLess) + "' against a 700px-wide device, " +
            "but the only breakpoint provided is for a value of 1200, then no breakpoint " + nameof(UiBreakpoint.Matched) + " events " +
            "will be raised, so this event will be raised instead."
        )]
        public UnityEvent NoBreakpointMatched = new UnityEvent();

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private new void Reset() {
            Mode = BreakpointMode.SafeAreaAspectRatio;
            MatchMode = BreakpointMatchMode.MinEqualOrGreater;

            RecheckMatchesOnResize = true;
        }

        protected override void Start() {
            base.Start();

            // Using Start rather than Awake cause we don't want to mess with breakpoints being raised by Awake during Edit Mode tests
            // Awake is called by AddComponent since we've added the ExecuteAlwaysAttribute to this class

            if (IsCameraMode)
                this.AssertAssociation(Camera, nameof(Camera));

            // Get the UI breakpoints and display value (height, width, etc.) requested for consideration
            _currentValue = getModeValue(Mode);
            InvokeMatchingBreakpoints(_currentValue);
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            if (RecheckMatchesOnResize) {
                _currentValue = getModeValue(Mode);
                InvokeMatchingBreakpoints(_currentValue);
            }
        }

        public bool IsCameraMode => Mode == BreakpointMode.CameraWidth || Mode == BreakpointMode.CameraHeight || Mode == BreakpointMode.CameraAspectRatio;
        public bool IsWidthMode => Mode == BreakpointMode.ScreenWidth || Mode == BreakpointMode.SafeAreaWidth || Mode == BreakpointMode.CameraWidth;
        public bool IsHeightMode => Mode == BreakpointMode.ScreenHeight || Mode == BreakpointMode.SafeAreaHeight || Mode == BreakpointMode.CameraHeight;
        public bool IsAspectRatioMode => Mode == BreakpointMode.ScreenAspectRatio || Mode == BreakpointMode.SafeAreaAspectRatio || Mode == BreakpointMode.CameraAspectRatio;

        internal static bool AreBreakpointsValid(UiBreakpoint[] breakpoints) {
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

        private float getModeValue(BreakpointMode mode) =>
            mode switch {
                BreakpointMode.ScreenWidth => Screen.width,
                BreakpointMode.ScreenHeight => Screen.height,
                BreakpointMode.ScreenAspectRatio => (float)Screen.width / Screen.height,

                BreakpointMode.SafeAreaWidth => Screen.safeArea.width,
                BreakpointMode.SafeAreaHeight => Screen.safeArea.height,
                BreakpointMode.SafeAreaAspectRatio => Screen.safeArea.width / Screen.safeArea.height,

                BreakpointMode.CameraWidth => Camera.pixelWidth,
                BreakpointMode.CameraHeight => Camera.pixelHeight,
                BreakpointMode.CameraAspectRatio => Camera.aspect,

                _ => throw UnityObjectExtensions.SwitchDefaultException(Mode),
            };

        internal const string MsgNegativeModeValue = "UI breakpoints can only be matched against non-negative values. You can ignore this warning if you just restarted the Editor.";

        internal void InvokeMatchingBreakpoints(float modeValue) {
            if (modeValue < 0f) {
                Debug.LogWarning(MsgNegativeModeValue);
                return;
            }

            // Early exit if no breakpoints were provided
            if (Breakpoints.Length == 0)
                return;

            // Reset all UI breakpoints to not matched state
            _noMatch = true;
            for (int b = 0; b < Breakpoints.Length; ++b)
                Breakpoints[b].IsMatched = false;

            // Raise the "matched" event on all matching breakpoints, according to match criteria...
            if (MatchMode == BreakpointMatchMode.MaxEqualOrLess)
            {
                UiBreakpoint breakpoint = Breakpoints.LastOrDefault(b => b.Enabled && b.Value <= modeValue);
                if (breakpoint != null)
                    invokeBreakpoint(breakpoint);
            }
            else
            {
                for (int b = 0; b < Breakpoints.Length; ++b)
                {
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


            void invokeBreakpoint(UiBreakpoint breakpoint) {
                breakpoint.IsMatched = true;
                breakpoint.Matched.Invoke();
                _noMatch = false;
            }
        }

    }
}
