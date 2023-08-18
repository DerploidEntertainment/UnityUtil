using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityUtil.Legal;
using UnityUtil.Logging;
using UnityUtil.Storage;
using S = System.Threading.Tasks;
using U = UnityEngine;

namespace UnityUtil.Editor.Tests.Legal
{
    public class SingleDialogConsentManagerTest : BaseEditModeTestFixture
    {
        private const string CONSENT_PREF_KEY_PREFIX = "TEST_CONSENT_";
        private static IEnumerable<TestCaseData> getConsentStateTestCases() =>
            new TestCaseData[] {
                new(LegalAcceptance.Unprovided, new bool?[]{ null }),
                new(LegalAcceptance.Unprovided, new bool?[]{ true }),
                new(LegalAcceptance.Unprovided, new bool?[]{ false }),
                new(LegalAcceptance.Unprovided, new bool?[]{ null, null }),
                new(LegalAcceptance.Unprovided, new bool?[]{ null, false }),
                new(LegalAcceptance.Unprovided, new bool?[]{ null, true }),
                new(LegalAcceptance.Unprovided, new bool?[]{ false, null }),
                new(LegalAcceptance.Unprovided, new bool?[]{ false, false }),
                new(LegalAcceptance.Unprovided, new bool?[]{ false, true }),
                new(LegalAcceptance.Unprovided, new bool?[]{ true, null }),
                new(LegalAcceptance.Unprovided, new bool?[]{ true, false }),
                new(LegalAcceptance.Unprovided, new bool?[]{ true, true }),
                new(LegalAcceptance.Stale, new bool?[]{ null }),
                new(LegalAcceptance.Stale, new bool?[]{ true }),
                new(LegalAcceptance.Stale, new bool?[]{ false }),
                new(LegalAcceptance.Stale, new bool?[]{ null, null }),
                new(LegalAcceptance.Stale, new bool?[]{ null, false }),
                new(LegalAcceptance.Stale, new bool?[]{ null, true }),
                new(LegalAcceptance.Stale, new bool?[]{ false, null }),
                new(LegalAcceptance.Stale, new bool?[]{ false, false }),
                new(LegalAcceptance.Stale, new bool?[]{ false, true }),
                new(LegalAcceptance.Stale, new bool?[]{ true, null }),
                new(LegalAcceptance.Stale, new bool?[]{ true, false }),
                new(LegalAcceptance.Stale, new bool?[]{ true, true }),
                new(LegalAcceptance.Current, new bool?[]{ null }),
                new(LegalAcceptance.Current, new bool?[]{ true }),
                new(LegalAcceptance.Current, new bool?[]{ false }),
                new(LegalAcceptance.Current, new bool?[]{ null, null }),
                new(LegalAcceptance.Current, new bool?[]{ null, false }),
                new(LegalAcceptance.Current, new bool?[]{ null, true }),
                new(LegalAcceptance.Current, new bool?[]{ false, null }),
                new(LegalAcceptance.Current, new bool?[]{ false, false }),
                new(LegalAcceptance.Current, new bool?[]{ false, true }),
                new(LegalAcceptance.Current, new bool?[]{ true, null }),
                new(LegalAcceptance.Current, new bool?[]{ true, false }),
                new(LegalAcceptance.Current, new bool?[]{ true, true }),
            };

        #pragma warning disable IDE1006 // Naming Styles    // Test names don't need the -Async suffix

        #region ShowDialogIfNeededAsync

        [Test]
        [TestCase(false, 0)]
        [TestCase(false, 1)]
        [TestCase(false, 2)]
        [TestCase(true, 0)]
        [TestCase(true, 1)]
        [TestCase(true, 2)]
        public async Task ShowDialogIfNeededAsync_ChecksLocalPrefs_ForEachConsent_IgnoringForceConsentBehavior(bool forceConsentBehavior, int consentCount)
        {
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(consentCount);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray()
            );
            singleDialogConsentManager.ForceConsentBehavior = forceConsentBehavior;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();

            foreach (Mock<IInitializableWithConsent> initializable in initializables)
                initializable.Verify(x => x.ConsentPreferenceKey, Times.Once);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task ShowDialogIfNeededAsync_ChecksLegalAcceptance_IgnoringForceConsentBehavior(bool forceConsentBehavior)
        {
            Mock<ILegalAcceptManager> legalAcceptManager = new();
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(legalAcceptManager: legalAcceptManager.Object);
            singleDialogConsentManager.ForceConsentBehavior = forceConsentBehavior;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();

            legalAcceptManager.Verify(x => x.CheckAcceptanceAsync(), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task ShowDialogIfNeededAsync_ShowsCorrectUi_BasedOnConsents(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            int initialFtueInvokeCount = 0;
            int legalUpdateInvokeCount = 0;
            int noUiInvokeCount = 0;
            bool someConsentRequired = priorConsents.Any(x => x is null);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManagerWithConsents(legalAcceptance, priorConsents);
            singleDialogConsentManager.ForceConsentBehavior = true;
            singleDialogConsentManager.InitialConsentRequired.AddListener(() => ++initialFtueInvokeCount);
            singleDialogConsentManager.LegalUpdateRequired.AddListener(() => ++legalUpdateInvokeCount);
            singleDialogConsentManager.NoUiRequired.AddListener(() => ++noUiInvokeCount);

            await singleDialogConsentManager.ShowDialogIfNeededAsync();

            Assert.That(initialFtueInvokeCount, Is.EqualTo(someConsentRequired || legalAcceptance == LegalAcceptance.Unprovided ? 1 : 0));
            Assert.That(legalUpdateInvokeCount, Is.EqualTo(!someConsentRequired && legalAcceptance == LegalAcceptance.Stale ? 1 : 0));
            Assert.That(noUiInvokeCount, Is.EqualTo(!someConsentRequired && legalAcceptance == LegalAcceptance.Current ? 1 : 0));
        }

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task ShowDialogIfNeededAsync_ShowsCorrectUi_DontForceConsentBehavior(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            int initialFtueInvokeCount = 0;
            int legalUpdateInvokeCount = 0;
            int noUiInvokeCount = 0;
            bool someConsentRequired = priorConsents.Any(x => x is null);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManagerWithConsents(legalAcceptance, priorConsents);
            singleDialogConsentManager.ForceConsentBehavior = false;
            singleDialogConsentManager.InitialConsentRequired.AddListener(() => ++initialFtueInvokeCount);
            singleDialogConsentManager.LegalUpdateRequired.AddListener(() => ++legalUpdateInvokeCount);
            singleDialogConsentManager.NoUiRequired.AddListener(() => ++noUiInvokeCount);

            await singleDialogConsentManager.ShowDialogIfNeededAsync();

            Assert.That(initialFtueInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Unprovided ? 1 : 0));
            Assert.That(legalUpdateInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Stale ? 1 : 0));
            Assert.That(noUiInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Current ? 1 : 0));
        }

        #endregion

        #region GiveConsent

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task GiveConsent_OnlySetsUpdatedConsents_ForceConsentBehavior(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
            U.Debug.Log($"Giving consent when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(priorConsents.Length);
            Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray(),
                localPreferences: localPreferences.Object
            );
            singleDialogConsentManager.ForceConsentBehavior = true;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();

            for (int index = 0; index < priorConsents.Length; index++) {
                string key = initializables[index].Object.ConsentPreferenceKey;
                if (priorConsents[index].HasValue)
                    localPreferences.Verify(x => x.SetInt(key, It.IsAny<int>()), Times.Never);
                else
                    localPreferences.Verify(x => x.SetInt(key, 1), Times.Once);
            }
        }

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task GiveConsent_OnlySetsUpdatedConsents_DontForceConsentBehavior(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
            U.Debug.Log($"Giving consent when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(priorConsents.Length);
            Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray(),
                localPreferences: localPreferences.Object
            );
            singleDialogConsentManager.ForceConsentBehavior = false;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();

            foreach (Mock<IInitializableWithConsent> initializable in initializables)
                localPreferences.Verify(x => x.SetInt(initializable.Object.ConsentPreferenceKey, It.IsAny<int>()), Times.Never);
        }

        [Test]
        [TestCase(false, LegalAcceptance.Unprovided)]
        [TestCase(false, LegalAcceptance.Stale)]
        [TestCase(false, LegalAcceptance.Current)]
        [TestCase(true, LegalAcceptance.Unprovided)]
        [TestCase(true, LegalAcceptance.Stale)]
        [TestCase(true, LegalAcceptance.Current)]
        public async Task GiveConsent_SetsLegalAcceptance_IfNotCurrent(bool forceConsentBehavior, LegalAcceptance legalAcceptance)
        {
            U.Debug.Log($"Giving consent when {nameof(forceConsentBehavior)} is {forceConsentBehavior} and {nameof(legalAcceptance)} is '{legalAcceptance}'...");
            Mock<ILegalAcceptManager> legalAcceptManager = getLegalAcceptManager(legalAcceptance);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(legalAcceptManager: legalAcceptManager.Object);
            singleDialogConsentManager.ForceConsentBehavior = forceConsentBehavior;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();

            legalAcceptManager.Verify(x => x.Accept(), legalAcceptance == LegalAcceptance.Current ? Times.Never : Times.Once);
        }

        #endregion

        #region Initialize

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task Initialize_InitializesAll_WithCorrectConsents_ForceConsentBehavior(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
            U.Debug.Log($"Initializing when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(priorConsents.Length);
            Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray(),
                localPreferences: localPreferences.Object
            );
            singleDialogConsentManager.ForceConsentBehavior = true;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();
            singleDialogConsentManager.Initialize();

            for (int index = 0; index < initializables.Length; index++) {
                Mock<IInitializableWithConsent> initializable = initializables[index];
                bool? priorConsent = priorConsents[index];
                initializable.Verify(x => x.InitializeAsync(priorConsent ?? true), Times.Once);
            }
        }

        [Test]
        [TestCaseSource(nameof(getConsentStateTestCases))]
        public async Task Initialize_InitializesAll_WithCorrectConsents_DontForceConsentBehavior(LegalAcceptance legalAcceptance, bool?[] priorConsents)
        {
            string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
            U.Debug.Log($"Initializing when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(priorConsents.Length);
            Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray(),
                localPreferences: localPreferences.Object
            );
            singleDialogConsentManager.ForceConsentBehavior = false;

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();
            singleDialogConsentManager.Initialize();

            for (int index = 0; index < initializables.Length; index++) {
                Mock<IInitializableWithConsent> initializable = initializables[index];
                bool? priorConsent = priorConsents[index];
                initializable.Verify(x => x.InitializeAsync(priorConsent ?? false), Times.Once);
            }
        }

        [Test]
        [TestCase(new[] { 10, 10, 10 })]
        [TestCase(new[] { 10, 20, 30 })]
        public async Task Initialize_InitializesAllInParallel(int[] initializeDurationsMs)
        {
            Mock<IInitializableWithConsent>[] initializables = getInitializablesWithConsent(initializeDurationsMs.Length, initializeDurationsMs);
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray()
            );

            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();
            var sw = Stopwatch.StartNew();
            singleDialogConsentManager.Initialize();
            sw.Stop();

            Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(initializeDurationsMs.Sum()));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task Initialize_DoesNotThrow_ButLogsExceptions(int consentCount)
        {
            // ARRANGE
            Mock<IInitializableWithConsent>[] initializables = Enumerable
                .Range(0, consentCount)
                .Select(x => {
                    Mock<IInitializableWithConsent> initializable = new();
                    initializable.Setup(x => x.InitializeAsync(It.IsAny<bool>()))
                        .ThrowsAsync(new InvalidOperationException($"AH!! Consent {x} exploded!"));
                    return initializable;
                })
                .ToArray();

            Exception? loggedException = null;
            SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
                loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, log),
                initializablesWithConsent: initializables.Select(x => x.Object).ToArray()
            );

            static void log(LogLevel logLevel, EventId eventId, Exception exception, string message)
            {
                LogType logType = logLevel switch {
                    LogLevel.None or LogLevel.Trace or LogLevel.Debug or LogLevel.Information => LogType.Log,
                    LogLevel.Warning => LogType.Warning,
                    LogLevel.Error or LogLevel.Critical => LogType.Error,
                    _ => throw UnityObjectExtensions.SwitchDefaultException(logLevel),
                };
                U.Debug.unityLogger.Log(logType, message);
            }

            // ACT
            await singleDialogConsentManager.ShowDialogIfNeededAsync();
            singleDialogConsentManager.GiveConsent();
            singleDialogConsentManager.Initialize();

            // ASSERT
            if (consentCount == 0) {
                Assert.That(loggedException, Is.Null);
            }
            else {
                Assert.That(loggedException, Is.Not.Null);
                Assert.That(((AggregateException)loggedException!).InnerExceptions, Has.None.TypeOf<AggregateException>()); // AggregateException has been flattened
                LogAssert.Expect(LogType.Error, new Regex(".+"));
            }
        }

        #endregion

#pragma warning restore IDE1006 // Naming Styles

        private static SingleDialogConsentManager getSingleDialogConsentManagerWithConsents(LegalAcceptance legalAcceptance, bool?[] priorConsents) =>
            getSingleDialogConsentManager(
                legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
                localPreferences: getLocalPreferences(priorConsents).Object,
                initializablesWithConsent: getInitializablesWithConsent(priorConsents.Length).Select(x => x.Object).ToArray()
            );

        private static SingleDialogConsentManager getSingleDialogConsentManager(
            ILoggerFactory? loggerFactory = null,
            ILegalAcceptManager? legalAcceptManager = null,
            ILocalPreferences? localPreferences = null,
            IEnumerable<IInitializableWithConsent>? initializablesWithConsent = null
        )
        {
            SingleDialogConsentManager singleDialogConsentManager = new GameObject().AddComponent<SingleDialogConsentManager>();
            singleDialogConsentManager.Inject(
                loggerFactory ?? new UnityDebugLoggerFactory(),
                legalAcceptManager ?? getLegalAcceptManager(LegalAcceptance.Unprovided).Object,
                localPreferences ?? getLocalPreferences().Object,
                initializablesWithConsent ?? Array.Empty<IInitializableWithConsent>()
            );
            return singleDialogConsentManager;
        }

        private static Mock<ILegalAcceptManager> getLegalAcceptManager(LegalAcceptance legalAcceptance)
        {
            Mock<ILegalAcceptManager> legalAcceptManager = new();
            legalAcceptManager.Setup(x => x.CheckAcceptanceAsync()).ReturnsAsync(legalAcceptance);
            return legalAcceptManager;
        }

        private static Mock<ILocalPreferences> getLocalPreferences(bool?[]? priorConsents = null)
        {
            Mock<ILocalPreferences> localPreferences = new();
            if (priorConsents is null)
                return localPreferences;

            for (int x = 0; x < priorConsents.Length; x++) {
                bool? priorConsent = priorConsents[x];
                string key = $"{CONSENT_PREF_KEY_PREFIX}{x}";
                localPreferences.Setup(x => x.HasKey(key)).Returns(priorConsent.HasValue);
                if (priorConsent.HasValue)
                    localPreferences.Setup(x => x.GetInt(key)).Returns(priorConsent!.Value ? 1 : 0);
            }

            return localPreferences;
        }

        private static Mock<IInitializableWithConsent>[] getInitializablesWithConsent(int consentCount, int[]? initializeDurations = null)
        {
            initializeDurations ??= new int[consentCount];
            return Enumerable
                .Range(0, consentCount)
                .Select(x => {
                    Mock<IInitializableWithConsent> initializable = new();
                    initializable.SetupGet(x => x.ConsentPreferenceKey).Returns($"{CONSENT_PREF_KEY_PREFIX}{x}");
                    initializable.Setup(x => x.InitializeAsync(It.IsAny<bool>())).Returns(S.Task.Delay(millisecondsDelay: initializeDurations[x]));
                    return initializable;
                })
                .ToArray();
        }
    }
}
