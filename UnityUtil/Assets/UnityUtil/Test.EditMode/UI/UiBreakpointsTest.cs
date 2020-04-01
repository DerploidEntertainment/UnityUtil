using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.UI;
using UnityUtil.Editor;
using UA = UnityEngine.Assertions;

namespace UnityUtil.Test.EditMode.UI {
    public class UiBreakpointsTest
    {
        [Test]
        public void ReturnsCorrect_IsCameraMode() {
            EditModeTestHelpers.ResetScene();

            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            assert(BreakpointMode.CameraWidth, true);
            assert(BreakpointMode.CameraHeight, true);
            assert(BreakpointMode.CameraAspectRatio, true);

            assert(BreakpointMode.ScreenWidth, false);
            assert(BreakpointMode.ScreenHeight, false);
            assert(BreakpointMode.ScreenAspectRatio, false);
            assert(BreakpointMode.SafeAreaWidth, false);
            assert(BreakpointMode.SafeAreaHeight, false);
            assert(BreakpointMode.SafeAreaAspectRatio, false);


            void assert(BreakpointMode mode, bool assertion) {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsCameraMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsWidthMode() {
            EditModeTestHelpers.ResetScene();

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


            void assert(BreakpointMode mode, bool assertion) {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsWidthMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsHeightMode() {
            EditModeTestHelpers.ResetScene();

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


            void assert(BreakpointMode mode, bool assertion) {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsHeightMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void ReturnsCorrect_IsAspectRatioMode() {
            EditModeTestHelpers.ResetScene();

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


            void assert(BreakpointMode mode, bool assertion) {
                uiBreakpoints.Mode = mode;
                Assert.That(uiBreakpoints.IsAspectRatioMode, assertion ? Is.True : (Constraint)Is.False);
            }
        }

        [Test]
        public void CannotConstructUiBreakpoint_NegativeValue() {
            Assert.Throws<UA.AssertionException>(() => new UiBreakpoint(-2f));
            Assert.Throws<UA.AssertionException>(() => new UiBreakpoint(-1f));
            Assert.DoesNotThrow(() => new UiBreakpoint(0f));
            Assert.DoesNotThrow(() => new UiBreakpoint(1f));
        }

        [Test]
        public void CorrectlyValidates_SortedBreakpoints() {
            EditModeTestHelpers.ResetScene();

            UiBreakpoint[] breakpoints;

            breakpoints = getBreakpoints(0f);
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(1f);
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(5f, 10f);
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(0f, 10f, 20f);
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(0f, 1f, 2f);
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));


            UiBreakpoint[] getBreakpoints(params float[] values) => values.Select(val => new UiBreakpoint(val)).ToArray();
        }

        [Test]
        public void CorrectlyValidates_UnsortedBreakpoints() {
            EditModeTestHelpers.ResetScene();

            UiBreakpoint[] breakpoints;

            // Breakpoints not sorted ascending
            breakpoints = getBreakpoints(1f, 1f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(1f, 5f, 5f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(10f, 5f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(0f, 20f, 10f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(20f, 10f, 0f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(0f, 2f, 1f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));

            breakpoints = getBreakpoints(2f, 1f, 0f);
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));


            UiBreakpoint[] getBreakpoints(params float[] values) => values.Select(val => new UiBreakpoint(val)).ToArray();
        }

        [Test]
        public void CorrectlyValidates_SortedBreakpoints_SomeDisabled() {
            EditModeTestHelpers.ResetScene();

            UiBreakpoint[] breakpoints;

            // Sorted with some disabled
            breakpoints = new[] {
                new UiBreakpoint(0f),
                new UiBreakpoint(0.4f, enabled: false),
                new UiBreakpoint(0.6f),
                new UiBreakpoint(1f, enabled: false),
            };
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            // Unsorted, but out-of-order ones disabled
            breakpoints = new[] {
                new UiBreakpoint(0f),
                new UiBreakpoint(1f, enabled: false),
                new UiBreakpoint(0.6f),
                new UiBreakpoint(0.4f, enabled: false),
            };
            Assert.IsTrue(UiBreakpoints.AreBreakpointsValid(breakpoints));

            // Unsorted, with one of the out-of-order ones re-enabled
            breakpoints[1].Enabled = true;
            Assert.IsFalse(UiBreakpoints.AreBreakpointsValid(breakpoints));


            UiBreakpoint[] getBreakpoints(params float[] values) => values.Select(val => new UiBreakpoint(val)).ToArray();
        }

        [Test]
        public void CannotMatchNegativeValue() {
            EditModeTestHelpers.ResetScene();

            UiBreakpoints uiBreakpoints = getUiBreakpoints();

            Assert.Throws<UA.AssertionException>(() => uiBreakpoints.InvokeMatchingBreakpoints(-1f));
            Assert.Throws<UA.AssertionException>(() => uiBreakpoints.InvokeMatchingBreakpoints(-2f));
        }

        [Test]
        public void NothingHappens_NoBreakpoints() {
            EditModeTestHelpers.ResetScene();

            bool noMatchRaised = false;
            UiBreakpoints uiBreakpoints = getUiBreakpoints();
            uiBreakpoints.NoBreakpointMatched.AddListener(() => noMatchRaised = true);

            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(0f));
            Assert.IsFalse(noMatchRaised);

            Assert.DoesNotThrow(() => uiBreakpoints.InvokeMatchingBreakpoints(10f));
            Assert.IsFalse(noMatchRaised);
        }

        [Test]
        public void NoMatches_NoMatch() {
            EditModeTestHelpers.ResetScene();

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
            Assert.IsFalse(matchRaised);
            Assert.IsTrue(noMatchRaised);


            void noMatchAction() => noMatchRaised = true;
        }

        [Test]
        public void NoMatches_MatchingBreakpointDisabled() {
            EditModeTestHelpers.ResetScene();

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
            Assert.IsFalse(matchRaised);
            Assert.IsTrue(noMatchRaised);


            void noMatchAction() => noMatchRaised = true;
        }

        [Test]
        public void NoMatch_AllBreakpointsLess() {
            EditModeTestHelpers.ResetScene();

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
            Assert.IsFalse(matchRaised);
            Assert.That(noMatchCount, Is.EqualTo(1));

            // ACT/ASSERT - MinEqualOrGreater
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.AnyEqualOrGreater, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(1f);
            Assert.IsFalse(matchRaised);
            Assert.That(noMatchCount, Is.EqualTo(2));


            void noMatchAction() => ++noMatchCount;
        }

        [Test]
        public void NoMatch_AllBreakpointsGreater() {
            EditModeTestHelpers.ResetScene();

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
            Assert.IsFalse(matchRaised);
            Assert.That(noMatchCount, Is.EqualTo(1));

            // ACT/ASSERT - MaxEqualOrLess
            uiBreakpoints = getUiBreakpoints(matchMode: BreakpointMatchMode.MaxEqualOrLess, noMatchAction: noMatchAction, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);
            Assert.IsFalse(matchRaised);
            Assert.That(noMatchCount, Is.EqualTo(2));


            void noMatchAction() => ++noMatchCount;
        }

        [Test]
        public void NoMatchNotRaised_SomeBreakpointMatches() {
            EditModeTestHelpers.ResetScene();

            // ARRANGE
            bool noMatchRaised = false;
            UiBreakpoint[] breakpoints = new[] { new UiBreakpoint(0f) };

            // ACT
            UiBreakpoints uiBreakpoints = getUiBreakpoints(noMatchAction: () => noMatchRaised = true, breakpoints: breakpoints);
            uiBreakpoints.InvokeMatchingBreakpoints(0f);

            // ASSERT
            Assert.IsFalse(noMatchRaised);
        }

        [Test]
        public void Match_1BreakpointEqualOrGreater() {
            EditModeTestHelpers.ResetScene();

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
        public void Match_1BreakpointEqualOrLess() {
            EditModeTestHelpers.ResetScene();

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
        public void CorrectMatch_MultipleBreakpoints_AnyEqualOrGreater() {
            EditModeTestHelpers.ResetScene();

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
        public void CorrectMatch_MultipleBreakpoints_MinEqualOrGreater() {
            EditModeTestHelpers.ResetScene();

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
        public void CorrectMatch_MultipleBreakpoints_AnyEqualOrLess() {
            EditModeTestHelpers.ResetScene();

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
        public void CorrectMatch_MultipleBreakpoints_MaxEqualOrLess() {
            EditModeTestHelpers.ResetScene();

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
        public void CorrectMatch_MultipleBreakpoints_SomeDisabled() {
            EditModeTestHelpers.ResetScene();

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

        private UiBreakpoints getUiBreakpoints(
            BreakpointMode mode = BreakpointMode.SafeAreaAspectRatio,
            BreakpointMatchMode matchMode = BreakpointMatchMode.MaxEqualOrLess,
            UnityAction noMatchAction = null,
            params UiBreakpoint[] breakpoints
        ) {
            var obj = new GameObject();

            // Set up dependencies
            ILoggerProvider loggerProvider = Mock.Of<ILoggerProvider>(x => x.GetLogger(It.IsAny<object>()) == Debug.unityLogger);

            // Create the instance
            UiBreakpoints uiBreakpoints = obj.AddComponent<UiBreakpoints>();
            uiBreakpoints.Inject(loggerProvider);
            uiBreakpoints.Mode = mode;
            uiBreakpoints.MatchMode = matchMode;
            uiBreakpoints.Breakpoints = breakpoints;
            if (noMatchAction != null)
                uiBreakpoints.NoBreakpointMatched.AddListener(noMatchAction);

            return uiBreakpoints;
        }

    }
}
