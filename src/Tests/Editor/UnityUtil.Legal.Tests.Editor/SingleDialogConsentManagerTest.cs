using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.TestTools;
using UnityUtil.Editor.Tests;
using UnityUtil.Storage;
using UnityUtil.Tests.Util;
using U = UnityEngine;

namespace UnityUtil.Legal.Tests.Editor;

public class SingleDialogConsentManagerTest : BaseEditModeTestFixture
{
    private const string CONSENT_PREF_KEY_PREFIX = "TEST_CONSENT_";

    private static TestCaseData[] getConsentStateTestCases() =>
        [
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
        ];

#pragma warning disable IDE1006 // Naming Styles    // Test names don't need the -Async suffix

    #region ShowDialogIfNeededAsync

    [Test]
    public async Task ShowDialogIfNeededAsync_ChecksLocalPrefs_ForEachConsent([Values(0, 1, 2)] int consentCount)
    {
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(consentCount);
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)]
        );

        await singleDialogConsentManager.ShowDialogIfNeededAsync();

        foreach (Mock<INonTcfDataProcessor> nonTcfDataProcessor in nonTcfDataProcessors)
            nonTcfDataProcessor.Verify(x => x.ConsentPreferenceKey, Times.Once);
    }

    [Test]
    public async Task ShowDialogIfNeededAsync_ChecksLegalAcceptance()
    {
        Mock<ILegalAcceptManager> legalAcceptManager = new();
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(legalAcceptManager: legalAcceptManager.Object);

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
        singleDialogConsentManager.InitialConsentRequired.AddListener(() => ++initialFtueInvokeCount);
        singleDialogConsentManager.LegalUpdateRequired.AddListener(() => ++legalUpdateInvokeCount);
        singleDialogConsentManager.NoUiRequired.AddListener(() => ++noUiInvokeCount);

        await singleDialogConsentManager.ShowDialogIfNeededAsync();

        Assert.That(initialFtueInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Unprovided ? 1 : 0));
        Assert.That(legalUpdateInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Stale ? 1 : 0));
        Assert.That(noUiInvokeCount, Is.EqualTo(legalAcceptance == LegalAcceptance.Current ? 1 : 0));
    }

    #endregion

    #region ContinueWithNonCmpConsentAsync

    [Test]
    [TestCaseSource(nameof(getConsentStateTestCases))]
    public async Task ContinueWithNonCmpConsentAsync_ShowsCmpConsentFormIfNecessary(LegalAcceptance legalAcceptance, bool?[] priorConsents)
    {
        // ARRANGE
        string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
        U.Debug.Log($"Giving consent when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
        Mock<ITcfCmpAdapter> tcfCmpAdapter = new();
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object,
            tcfCmpAdapter: tcfCmpAdapter.Object
        );

        // ACT
        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        if (legalAcceptance != LegalAcceptance.Current || priorConsents.Any(x => x is null))
            await singleDialogConsentManager.ContinueWithNonCmpConsentAsync();

        // ASSERT
        tcfCmpAdapter.Verify(x => x.LoadAndShowConsentFormIfRequiredAsync(), Times.Once());
    }

    [Test]
    [TestCaseSource(nameof(getConsentStateTestCases))]
    public async Task ContinueWithNonCmpConsentAsync_OnlySavesUpdatedNonCmpConsents(LegalAcceptance legalAcceptance, bool?[] priorConsents)
    {
        // ARRANGE
        string consentStrings = string.Join(",", priorConsents.Select(x => x?.ToString() ?? "Null"));
        U.Debug.Log($"Giving consent when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents);
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        // ACT
        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        if (legalAcceptance != LegalAcceptance.Current || priorConsents.Any(x => x is null))
            await singleDialogConsentManager.ContinueWithNonCmpConsentAsync();

        // ASSERT
        for (int index = 0; index < priorConsents.Length; index++) {
            string key = nonTcfDataProcessors[index].Object.ConsentPreferenceKey;
            if (priorConsents[index].HasValue)
                localPreferences.Verify(x => x.SetInt(key, It.IsAny<int>()), Times.Never);
            else
                localPreferences.Verify(x => x.SetInt(key, 1), Times.Once);
        }
    }

    [Test]
    public async Task ContinueWithNonCmpConsentAsync_SetsLegalAcceptance_IfNotCurrent([Values] LegalAcceptance legalAcceptance)
    {
        // ARRANGE
        U.Debug.Log($"Giving consent when {nameof(legalAcceptance)} is '{legalAcceptance}'...");
        Mock<ILegalAcceptManager> legalAcceptManager = getLegalAcceptManager(legalAcceptance);
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(legalAcceptManager: legalAcceptManager.Object);

        // ACT
        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        if (legalAcceptance != LegalAcceptance.Current)
            await singleDialogConsentManager.ContinueWithNonCmpConsentAsync();

        // ASSERT
        legalAcceptManager.Verify(x => x.Accept(), legalAcceptance == LegalAcceptance.Current ? Times.Never : Times.Once);
    }

    [Test]
    [TestCaseSource(nameof(getConsentStateTestCases))]
    public async Task ContinueWithNonCmpConsentAsync_StartsNonTcfDataCollection_WithCorrectConsents(LegalAcceptance legalAcceptance, bool?[] priorNonCmpConsents)
    {
        // ARRANGE
        string consentStrings = string.Join(",", priorNonCmpConsents.Select(x => x?.ToString() ?? "Null"));
        U.Debug.Log($"Initializing when legal acceptance is '{legalAcceptance}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorNonCmpConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorNonCmpConsents);
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        // ACT
        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        if (legalAcceptance != LegalAcceptance.Current || priorNonCmpConsents.Any(x => x is null))
            await singleDialogConsentManager.ContinueWithNonCmpConsentAsync();

        // ASSERT
        for (int index = 0; index < nonTcfDataProcessors.Length; index++) {
            Mock<INonTcfDataProcessor> nonTcfDataProcessor = nonTcfDataProcessors[index];
            bool? priorConsent = priorNonCmpConsents[index];
            nonTcfDataProcessor.Verify(x => x.StartDataCollection(), priorConsent.HasValue && !priorConsent.Value ? Times.Never() : Times.Once());
        }
    }

    [Test]
    public async Task ContinueWithNonCmpConsentAsync_InitializesTcfDataProcessors([Values] LegalAcceptance legalAcceptance)
    {
        // ARRANGE
        Mock<ITcfDataProcessor>[] tcfDataProcessors = [
            new Mock<ITcfDataProcessor>(),
            new Mock<ITcfDataProcessor>()
        ];
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
            tcfDataProcessors: [.. tcfDataProcessors.Select(x => x.Object)]
        );

        // ACT
        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        if (legalAcceptance != LegalAcceptance.Current)
            await singleDialogConsentManager.ContinueWithNonCmpConsentAsync();

        // ASSERT
        foreach (Mock<ITcfDataProcessor> tcfDataProcessor in tcfDataProcessors)
            tcfDataProcessor.Verify(x => x.InitializeAsync(), Times.Once());
    }

    [Test]
    public async Task ContinueWithNonCmpConsentAsync_LogsNotThrows_NonTcfDataCollectionExceptions([Values(0, 1, 2)] int consentCount)
    {
        // ARRANGE
        Exception? loggedException = null;
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, log),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptance.Current).Object,
            localPreferences: getLocalPreferences(priorConsents: [.. Enumerable.Repeat(true, consentCount)]).Object,
            nonTcfDataProcessors: getNonTcfDataProcessors(consentCount).Select((x, index) => {
                _ = x.Setup(y => y.StartDataCollection()).Throws(new InvalidOperationException($"AH!! Consent {index} exploded!"));
                return x.Object;
            })
        );

        static void log(LogLevel logLevel, EventId eventId, Exception? exception, string message)
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

        // ASSERT
        if (consentCount == 0) {
            Assert.That(loggedException, Is.Null);
        }
        else {
            Assert.That(loggedException, Is.InstanceOf<InvalidOperationException>());
            LogAssert.Expect(LogType.Error, new Regex(".+"));
        }
    }

    [Test]
    public async Task ContinueWithNonCmpConsentAsync_LogsNotThrows_TcfDataProcessorInitializeExceptions([Values(0, 1, 2)] int consentCount)
    {
        // ARRANGE
        ITcfDataProcessor[] tcfDataProcessors = [.. Enumerable
            .Range(0, consentCount)
            .Select(x => {
                Mock<ITcfDataProcessor> tcfDataProcessor = new();
                _ = tcfDataProcessor.Setup(x => x.InitializeAsync()).ThrowsAsync(new InvalidOperationException($"AH!! Consent {x} exploded!"));
                return tcfDataProcessor.Object;
            })
        ];

        Exception? loggedException = null;
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, log),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptance.Current).Object,
            tcfDataProcessors: tcfDataProcessors
        );

        static void log(LogLevel logLevel, EventId eventId, Exception? exception, string message)
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

    #region WasConsentGranted

    [Test]
    public async Task WasConsentGranted_Throws_NonTcfDataProcessorNotFound()
    {
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = getNonTcfDataProcessor();
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(nonTcfDataProcessors: nonTcfDataProcessors);

        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        _ = Assert.Throws<ArgumentException>(() => singleDialogConsentManager.WasConsentGranted(nonTcfDataProcessor.Object));
    }

    [Test]
    public async Task WasConsentGranted_GetsCorrectConent([Values] bool hasConsent)
    {
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(1);
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: getLocalPreferences(priorConsents: [hasConsent]).Object
        );

        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        Assert.That(singleDialogConsentManager.WasConsentGranted(nonTcfDataProcessors[0].Object), Is.EqualTo(hasConsent));
    }

    #endregion

    #region RevokeConsent

    [Test]
    public async Task RevokeConsent_Throws_NonTcfDataProcessorNotFound()
    {
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = getNonTcfDataProcessor();
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(nonTcfDataProcessors: nonTcfDataProcessors);

        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        _ = Assert.Throws<ArgumentException>(() => singleDialogConsentManager.RevokeConsent(nonTcfDataProcessor.Object));
    }

    [Test]
    public async Task RevokeConsent_SetsLocalPreference()
    {
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<ILocalPreferences> localPreferences = getLocalPreferences();
        SingleDialogConsentManager singleDialogConsentManager = getSingleDialogConsentManager(
            nonTcfDataProcessors: nonTcfDataProcessors,
            localPreferences: localPreferences.Object
        );

        await singleDialogConsentManager.ShowDialogIfNeededAsync();
        singleDialogConsentManager.RevokeConsent(nonTcfDataProcessors[0]);

        localPreferences.Verify(x => x.SetInt(nonTcfDataProcessors[0].ConsentPreferenceKey, 0), Times.Once);
    }

    #endregion

#pragma warning restore IDE1006 // Naming Styles

    private static SingleDialogConsentManager getSingleDialogConsentManagerWithConsents(LegalAcceptance legalAcceptance, bool?[] priorConsents) =>
        getSingleDialogConsentManager(
            legalAcceptManager: getLegalAcceptManager(legalAcceptance).Object,
            localPreferences: getLocalPreferences(priorConsents).Object,
            nonTcfDataProcessors: [.. getNonTcfDataProcessors(priorConsents.Length).Select(x => x.Object)]
        );

    private static SingleDialogConsentManager getSingleDialogConsentManager(
        ILoggerFactory? loggerFactory = null,
        ILegalAcceptManager? legalAcceptManager = null,
        ILocalPreferences? localPreferences = null,
        ITcfCmpAdapter? tcfCmpAdapter = null,
        IEnumerable<ITcfDataProcessor>? tcfDataProcessors = null,
        IEnumerable<INonTcfDataProcessor>? nonTcfDataProcessors = null
    )
    {
        SingleDialogConsentManager singleDialogConsentManager = new GameObject().AddComponent<SingleDialogConsentManager>();
        singleDialogConsentManager.Inject(
            loggerFactory ?? new UnityDebugLoggerFactory(),
            legalAcceptManager ?? getLegalAcceptManager(LegalAcceptance.Unprovided).Object,
            localPreferences ?? getLocalPreferences().Object,
            tcfCmpAdapter ?? Mock.Of<ITcfCmpAdapter>(),
            tcfDataProcessors ?? [],
            nonTcfDataProcessors ?? []
        );
        return singleDialogConsentManager;
    }

    private static Mock<ILegalAcceptManager> getLegalAcceptManager(LegalAcceptance legalAcceptance)
    {
        Mock<ILegalAcceptManager> legalAcceptManager = new();
        _ = legalAcceptManager.Setup(x => x.CheckAcceptanceAsync()).ReturnsAsync(legalAcceptance);
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
            _ = localPreferences.Setup(x => x.HasKey(key)).Returns(priorConsent.HasValue);
            if (priorConsent.HasValue)
                _ = localPreferences.Setup(x => x.GetInt(key)).Returns(priorConsent!.Value ? 1 : 0);
        }

        return localPreferences;
    }

    private static Mock<INonTcfDataProcessor>[] getNonTcfDataProcessors(int consentCount) =>
        [.. Enumerable
            .Range(0, consentCount)
            .Select(x => {
                Mock<INonTcfDataProcessor> nonTcfDataProcessor = new();
                _ = nonTcfDataProcessor.SetupGet(x => x.ConsentPreferenceKey).Returns($"{CONSENT_PREF_KEY_PREFIX}{x}");
                return nonTcfDataProcessor;
            })
        ];

    private static Mock<INonTcfDataProcessor> getNonTcfDataProcessor()
    {
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = new();
        _ = nonTcfDataProcessor.SetupGet(x => x.ConsentPreferenceKey).Returns(CONSENT_PREF_KEY_PREFIX);
        return nonTcfDataProcessor;
    }
}
