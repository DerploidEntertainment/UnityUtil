using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Logging;
using UnityUtil.UI;
using UA = UnityEngine.Assertions;

namespace UnityUtil.Editor.Tests.UI
{
    public class UiBreakpointsTest : BaseEditModeTestFixture
    {
        [Test]
        public void ReturnsCorrect_IsScreenMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenWidth, true);
            assert(BreakpointMode.ScreenHeight, true);
            assert(BreakpointMode.ScreenAspectRatio, true);

            assert(BreakpointMode.SafeAreaWidth, false);
            assert(BreakpointMode.SafeAreaHeight, false);
            assert(BreakpointMode.SafeAreaAspectRatio, false);

            assert(BreakpointMode.CameraWidth, false);
            assert(BreakpointMode.CameraHeight, false);
            assert(BreakpointMode.CameraAspectRatio, false);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsScreenMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsSafeAreaMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenWidth, false);
            assert(BreakpointMode.ScreenHeight, false);
            assert(BreakpointMode.ScreenAspectRatio, false);

            assert(BreakpointMode.SafeAreaWidth, true);
            assert(BreakpointMode.SafeAreaHeight, true);
            assert(BreakpointMode.SafeAreaAspectRatio, true);

            assert(BreakpointMode.CameraWidth, false);
            assert(BreakpointMode.CameraHeight, false);
            assert(BreakpointMode.CameraAspectRatio, false);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsSafeAreaMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsCameraMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenWidth, false);
            assert(BreakpointMode.ScreenHeight, false);
            assert(BreakpointMode.ScreenAspectRatio, false);

            assert(BreakpointMode.SafeAreaWidth, false);
            assert(BreakpointMode.SafeAreaHeight, false);
            assert(BreakpointMode.SafeAreaAspectRatio, false);

            assert(BreakpointMode.CameraWidth, true);
            assert(BreakpointMode.CameraHeight, true);
            assert(BreakpointMode.CameraAspectRatio, true);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsCameraMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsWidthMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenWidth, true);
            assert(BreakpointMode.SafeAreaWidth, true);
            assert(BreakpointMode.CameraWidth, true);

            assert(BreakpointMode.ScreenHeight, false);
            assert(BreakpointMode.ScreenAspectRatio, false);
            assert(BreakpointMode.SafeAreaHeight, false);
            assert(BreakpointMode.SafeAreaAspectRatio, false);
            assert(BreakpointMode.CameraHeight, false);
            assert(BreakpointMode.CameraAspectRatio, false);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsWidthMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsHeightMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenHeight, true);
            assert(BreakpointMode.SafeAreaHeight, true);
            assert(BreakpointMode.CameraHeight, true);

            assert(BreakpointMode.ScreenWidth, false);
            assert(BreakpointMode.ScreenAspectRatio, false);
            assert(BreakpointMode.SafeAreaWidth, false);
            assert(BreakpointMode.SafeAreaAspectRatio, false);
            assert(BreakpointMode.CameraWidth, false);
            assert(BreakpointMode.CameraAspectRatio, false);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsHeightMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsAspectRatioMode()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.ScreenAspectRatio, true);
            assert(BreakpointMode.SafeAreaAspectRatio, true);
            assert(BreakpointMode.CameraAspectRatio, true);

            assert(BreakpointMode.ScreenWidth, false);
            assert(BreakpointMode.ScreenHeight, false);
            assert(BreakpointMode.SafeAreaWidth, false);
            assert(BreakpointMode.SafeAreaHeight, false);
            assert(BreakpointMode.CameraWidth, false);
            assert(BreakpointMode.CameraHeight, false);


            void assert(BreakpointMode mode, bool assertion)
            {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsAspectRatioMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void CannotConstructUiBreakpoint_NegativeValue()
        {
            Assert.Throws<UA.AssertionException>(() => new UiBreakpoint(-2f));
            Assert.Throws<UA.AssertionException>(() => new UiBreakpoint(-1f));
            Assert.DoesNotThrow(() => new UiBreakpoint(0f));
            Assert.DoesNotThrow(() => new UiBreakpoint(1f));
        }

        [Test]
        public void CorrectlyValidates_SortedBreakpoints()
        {
            UiBreakpoint[] breakpoints;

            breakpoints = getBreakpoints(0f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            breakpoints = getBreakpoints(1f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            breakpoints = getBreakpoints(5f, 10f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            breakpoints = getBreakpoints(0f, 10f, 20f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            breakpoints = getBreakpoints(0f, 1f, 2f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            static UiBreakpoint[] getBreakpoints(params float[] values) => values.Select(val => new UiBreakpoint(val)).ToArray();
        }

        [Test]
        public void CorrectlyValidates_UnsortedBreakpoints()
        {
            UiBreakpoint[] breakpoints;

            // Breakpoints not sorted ascending
            breakpoints = getBreakpoints(1f, 1f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(1f, 5f, 5f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(10f, 5f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(0f, 20f, 10f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(20f, 10f, 0f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(0f, 2f, 1f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            breakpoints = getBreakpoints(2f, 1f, 0f);
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);

            static UiBreakpoint[] getBreakpoints(params float[] values) => values.Select(val => new UiBreakpoint(val)).ToArray();
        }

        [Test]
        public void CorrectlyValidates_SortedBreakpoints_SomeDisabled()
        {
            UiBreakpoint[] breakpoints;

            // Sorted with some disabled
            breakpoints = new[] {
                new UiBreakpoint(0f),
                new UiBreakpoint(0.4f, enabled: false),
                new UiBreakpoint(0.6f),
                new UiBreakpoint(1f, enabled: false),
            };
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            // Unsorted, but out-of-order ones disabled
            breakpoints = new[] {
                new UiBreakpoint(0f),
                new UiBreakpoint(1f, enabled: false),
                new UiBreakpoint(0.6f),
                new UiBreakpoint(0.4f, enabled: false),
            };
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.True);

            // Unsorted, with one of the out-of-order ones re-enabled
            breakpoints[1].Enabled = true;
            Assert.That(UiBreakpoints.AreBreakpointsValid(breakpoints), Is.False);
        }

        [Test]
        public void CannotMatchNegativeValue()
        {
            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(-1f));
            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(-2f));
        }

        [Test]
        public void NothingHappens_NoBreakpoints()
        {
            bool noMatchRaised = false;
            UiBreakpoints uiBreakpoints = getUiBreakpoints();
            uiBreakpoints.NoBreakpointMatched.AddListener(() => noMatchRaised = true);

            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(0f));
            Assert.That(noMatchRaised, Is.False);

            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(10f));
            Assert.That(noMatchRaised, Is.False);
        }

        [Test]
        public void NoMatches_NoMatch()
        {
            // ARRANGE
            bool noMatchRaised = false;
            bool matchRaised = false;
            var breakpoint0 = new UiBreakpoint(0.4f);
            var breakpoint1 = new UiBreakpoint(0.6f);
            breakpoint0.Matched.AddListener(() => matchRaised = true);
            breakpoint1.Matched.AddListener(() => matchRaised = true);
            UiBreakpoint[] breakpoints = new[] { breakpoint0, breakpoint1 };

            // ACT
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);

            // ASSERT
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchRaised, Is.True);


            void noMatchAction() => noMatchRaised = true;
        }

        [Test]
        public void NoMatches_MatchingBreakpointDisabled()
        {
            // ARRANGE
            bool noMatchRaised = false;
            bool matchRaised = false;
            var breakpoint = new UiBreakpoint(0.4f, enabled: false);
            breakpoint.Matched.AddListener(() => matchRaised = true);
            UiBreakpoint[] breakpoints = new[] { breakpoint };

            // ACT
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);

            // ASSERT
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchRaised, Is.True);


            void noMatchAction() => noMatchRaised = true;
        }

        [Test]
        public void NoMatch_AllBreakpointsLess()
        {
            // ARRANGE
            bool matchRaised = false;
            int noMatchCount = 0;
            UiBreakpoints uiBreakpoints;
            var breakpoint = new UiBreakpoint(0.5f);
            breakpoint.Matched.AddListener(() => matchRaised = true);
            UiBreakpoint[] breakpoints = new[] { breakpoint };

            // ACT/ASSERT - AnyEqualOrGreater
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchCount, Is.EqualTo(1));

            // ACT/ASSERT - MinEqualOrGreater
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchCount, Is.EqualTo(2));


            void noMatchAction() => ++noMatchCount;
        }

        [Test]
        public void NoMatch_AllBreakpointsGreater()
        {
            // ARRANGE
            bool matchRaised = false;
            int noMatchCount = 0;
            UiBreakpoints uiBreakpoints;
            var breakpoint = new UiBreakpoint(0.5f);
            breakpoint.Matched.AddListener(() => matchRaised = true);
            UiBreakpoint[] breakpoints = new[] { breakpoint };

            // ACT/ASSERT - AnyEqualOrLess
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrLess, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchCount, Is.EqualTo(1));

            // ACT/ASSERT - MaxEqualOrLess
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MaxEqualOrLess, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);
            Assert.That(matchRaised, Is.False);
            Assert.That(noMatchCount, Is.EqualTo(2));


            void noMatchAction() => ++noMatchCount;
        }

        [Test]
        public void NoMatchNotRaised_SomeBreakpointMatches()
        {
            // ARRANGE
            bool noMatchRaised = false;
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f) };

            // ACT
            UiBreakpoints uiBreakpoints = getUiBreakpoints(noMatchAction: () => noMatchRaised = true, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);

            // ASSERT
            Assert.That(noMatchRaised, Is.False);
        }

        [Test]
        public void Match_1BreakpointEqualOrGreater()
        {
            // ARRANGE
            int matchCount = 0;
            UiBreakpoints uiBreakpoints;
            var breakpoint = new UiBreakpoint(0.5f);
            breakpoint.Matched.AddListener(() => ++matchCount);
            UiBreakpoint[] breakpoints = new[] { breakpoint };

            // ACT/ASSERT - AnyEqualOrGreater - Greater
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);
            Assert.That(matchCount, Is.EqualTo(1));

            // ACT/ASSERT - AnyEqualOrGreater - Equal
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(matchCount, Is.EqualTo(2));

            // ACT/ASSERT - MinEqualOrGreater - Greater
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MinEqualOrGreater, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);
            Assert.That(matchCount, Is.EqualTo(3));

            // ACT/ASSERT - MinEqualOrGreater - Equal
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MinEqualOrGreater, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(matchCount, Is.EqualTo(4));
        }

        [Test]
        public void Match_1BreakpointEqualOrLess()
        {
            // ARRANGE
            int matchCount = 0;
            UiBreakpoints uiBreakpoints;
            var breakpoint = new UiBreakpoint(0.5f);
            breakpoint.Matched.AddListener(() => ++matchCount);
            UiBreakpoint[] breakpoints = new[] { breakpoint };

            // ACT/ASSERT - AnyEqualOrLess - Less
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrLess, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);
            Assert.That(matchCount, Is.EqualTo(1));

            // ACT/ASSERT - AnyEqualOrLess - Equal
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrLess, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(matchCount, Is.EqualTo(2));

            // ACT/ASSERT - MaxEqualOrLess - Less
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MaxEqualOrLess, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);
            Assert.That(matchCount, Is.EqualTo(3));

            // ACT/ASSERT - MaxEqualOrLess - Equal
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MaxEqualOrLess, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(matchCount, Is.EqualTo(4));
        }

        [Test]
        public void CorrectMatch_MultipleBreakpoints_AnyEqualOrGreater()
        {
            // ARRANGE
            int[] counts = new[] { 0, 0, 0 };
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f), new UiBreakpoint(0.5f), new UiBreakpoint(1f), };
            for (int b = 0; b < breakpoints.Length; ++b) {
                int index = b;
                breakpoints[b].Matched.AddListener(() => ++counts[index]);
            }
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, breakpoints: breakpoints);

            // ACT/ASSERT
            uiBreakpoints.InvokeMatchingBreakpoints(0.4f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(1));
            Assert.That(counts[2], Is.EqualTo(1));

            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(2));

            uiBreakpoints.InvokeMatchingBreakpoints(0.6f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(3));
        }

        [Test]
        public void CorrectMatch_MultipleBreakpoints_MinEqualOrGreater()
        {
            // ARRANGE
            int[] counts = new[] { 0, 0, 0 };
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f), new UiBreakpoint(0.5f), new UiBreakpoint(1f), };
            for (int b = 0; b < breakpoints.Length; ++b) {
                int index = b;
                breakpoints[b].Matched.AddListener(() => ++counts[index]);
            }
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MinEqualOrGreater, breakpoints: breakpoints);

            // ACT/ASSERT - MinEqualOrGreater
            uiBreakpoints.InvokeMatchingBreakpoints(0.4f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(1));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.6f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(1));
        }

        [Test]
        public void CorrectMatch_MultipleBreakpoints_AnyEqualOrLess()
        {
            // ARRANGE
            int[] counts = new[] { 0, 0, 0 };
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f), new UiBreakpoint(0.5f), new UiBreakpoint(1f), };
            for (int b = 0; b < breakpoints.Length; ++b) {
                int index = b;
                breakpoints[b].Matched.AddListener(() => ++counts[index]);
            }
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrLess, breakpoints: breakpoints);

            // ACT/ASSERT
            uiBreakpoints.InvokeMatchingBreakpoints(0.6f);
            Assert.That(counts[0], Is.EqualTo(1));
            Assert.That(counts[1], Is.EqualTo(1));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(2));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.4f);
            Assert.That(counts[0], Is.EqualTo(3));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(0));
        }

        [Test]
        public void CorrectMatch_MultipleBreakpoints_MaxEqualOrLess()
        {
            // ARRANGE
            int[] counts = new[] { 0, 0, 0 };
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f), new UiBreakpoint(0.5f), new UiBreakpoint(1f), };
            for (int b = 0; b < breakpoints.Length; ++b) {
                int index = b;
                breakpoints[b].Matched.AddListener(() => ++counts[index]);
            }
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MaxEqualOrLess, breakpoints: breakpoints);

            // ACT/ASSERT - MinEqualOrGreater
            uiBreakpoints.InvokeMatchingBreakpoints(0.6f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(1));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(0));

            uiBreakpoints.InvokeMatchingBreakpoints(0.4f);
            Assert.That(counts[0], Is.EqualTo(1));
            Assert.That(counts[1], Is.EqualTo(2));
            Assert.That(counts[2], Is.EqualTo(0));
        }

        [Test]
        public void CorrectMatch_MultipleBreakpoints_SomeDisabled()
        {
            // ARRANGE
            int[] counts = new[] { 0, 0, 0, 0 };
            UiBreakpoint[] breakpoints = new[] {
                new UiBreakpoint(0f),
                new UiBreakpoint(0.4f, enabled: false),
                new UiBreakpoint(0.6f, enabled: false),
                new UiBreakpoint(1f),
            };
            for (int b = 0; b < breakpoints.Length; ++b) {
                int index = b;
                breakpoints[b].Matched.AddListener(() => ++counts[index]);
            }
            UiBreakpoints uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, breakpoints: breakpoints);

            // ACT/ASSERT
            uiBreakpoints.InvokeMatchingBreakpoints(0.3f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(0));
            Assert.That(counts[2], Is.EqualTo(0));
            Assert.That(counts[3], Is.EqualTo(1));

            uiBreakpoints.InvokeMatchingBreakpoints(0.4f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(0));
            Assert.That(counts[2], Is.EqualTo(0));
            Assert.That(counts[3], Is.EqualTo(2));

            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(0));
            Assert.That(counts[2], Is.EqualTo(0));
            Assert.That(counts[3], Is.EqualTo(3));

            breakpoints[2].Enabled = true;
            uiBreakpoints.InvokeMatchingBreakpoints(0.5f);
            Assert.That(counts[0], Is.EqualTo(0));
            Assert.That(counts[1], Is.EqualTo(0));
            Assert.That(counts[2], Is.EqualTo(1));
            Assert.That(counts[3], Is.EqualTo(4));
        }

        private static UiBreakpoints getUiBreakpoints(
            BreakpointMode mode = BreakpointMode.SafeAreaAspectRatio,
            BreakpointMatchMode matchMode = BreakpointMatchMode.MaxEqualOrLess,
            UnityAction? noMatchAction = null,
            params UiBreakpoint[] breakpoints
        )
        {
            // Create the instance
            UiBreakpoints uiBreakpoints = new GameObject().AddComponent<UiBreakpoints>();
            uiBreakpoints.Inject(new UnityDebugLoggerFactory());
            uiBreakpoints.Mode = mode;
            uiBreakpoints.MatchMode = matchMode;
            uiBreakpoints.Breakpoints = breakpoints;
            if (noMatchAction is not null)
                uiBreakpoints.NoBreakpointMatched.AddListener(noMatchAction);

            return uiBreakpoints;
        }

    }
}
