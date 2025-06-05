using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityUtil.Editor.Tests;
using UnityUtil.Storage;
using UnityUtil.Tests.Util;

namespace UnityUtil.Legal.Tests.Editor;

public class PrivacyDataProcessorsInitializerTests : BaseEditModeTestFixture
{
    private const string CONSENT_PREF_KEY_PREFIX = "TEST_CONSENT_";

    private static bool?[][] getPriorNonCmpConsents() => [
        [null],
        [true],
        [false],
        [null, null],
        [null, false],
        [null, true],
        [false, null],
        [false, false],
        [false, true],
        [true, null],
        [true, false],
        [true, true],
    ];

#pragma warning disable IDE1006 // Naming Styles    // Test names don't need the -Async suffix

    #region InitializeDataProcessorsWithConsentAsync

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ChecksLocalPrefs_ForEachConsent([Values(0, 1, 2)] int consentCount)
    {
        Mock<ILocalPreferences> localPreferences = new();
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(consentCount);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            localPreferences: localPreferences.Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)]
        );

        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        foreach (Mock<INonTcfDataProcessor> nonTcfDataProcessor in nonTcfDataProcessors)
            localPreferences.Verify(x => x.HasKey(nonTcfDataProcessor.Object.ConsentPreferenceKey), Times.Once());
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_CheckslegalAcceptStatus()
    {
        // ARRANGE
        IEnumerable<LegalDocument> actualLegalDocs = [];
        Mock<ILegalAcceptManager> legalAcceptManager = new();
        _ = legalAcceptManager.Setup(x => x.CheckStatusAsync(It.IsAny<IEnumerable<LegalDocument>>()))
            .Callback<IEnumerable<LegalDocument>>(x => actualLegalDocs = x);

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(legalAcceptManager: legalAcceptManager.Object);
        var expectedLegalDocs = new LegalDocument[] { ScriptableObject.CreateInstance<LegalDocument>() };
        privacyDataProcessorsInitializer.LegalDocuments = expectedLegalDocs;

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        legalAcceptManager.Verify(x => x.CheckStatusAsync(It.IsAny<IEnumerable<LegalDocument>>()), Times.Once());
        Assert.That(actualLegalDocs, Is.EqualTo(expectedLegalDocs));
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_RaisesCorrectEvents(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(getPriorNonCmpConsents))] bool?[] priorNonCmpConsents
    )
    {
        // ARRANGE
        int nonCmpConsentRequired_InvokeCount = 0;
        int nonCmpConsentNotRequired_InvokeCount = 0;
        int legalAcceptUnprovided_InvokeCount = 0;
        int legalAcceptStale_InvokeCount = 0;
        int legalAcceptCurrent_InvokeCount = 0;
        bool someNonCmpConsentRequired = priorNonCmpConsents.Any(x => x is null);

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer_WithConsents(legalAcceptStatus, priorNonCmpConsents);
        privacyDataProcessorsInitializer.NonCmpConsentRequired.AddListener(() => ++nonCmpConsentRequired_InvokeCount);
        privacyDataProcessorsInitializer.NonCmpConsentNotRequired.AddListener(() => ++nonCmpConsentNotRequired_InvokeCount);
        privacyDataProcessorsInitializer.LegalAcceptUnprovided.AddListener(() => ++legalAcceptUnprovided_InvokeCount);
        privacyDataProcessorsInitializer.LegalAcceptStale.AddListener(() => ++legalAcceptStale_InvokeCount);
        privacyDataProcessorsInitializer.LegalAcceptCurrent.AddListener(() => ++legalAcceptCurrent_InvokeCount);

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        Assert.That(nonCmpConsentRequired_InvokeCount, Is.EqualTo(someNonCmpConsentRequired ? 1 : 0));
        Assert.That(nonCmpConsentNotRequired_InvokeCount, Is.EqualTo(someNonCmpConsentRequired ? 0 : 1));
        Assert.That(legalAcceptUnprovided_InvokeCount, Is.EqualTo(legalAcceptStatus == LegalAcceptStatus.Unprovided ? 1 : 0));
        Assert.That(legalAcceptStale_InvokeCount, Is.EqualTo(legalAcceptStatus == LegalAcceptStatus.Stale ? 1 : 0));
        Assert.That(legalAcceptCurrent_InvokeCount, Is.EqualTo(legalAcceptStatus == LegalAcceptStatus.Current ? 1 : 0));
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ShowsCorrectUi(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(getPriorNonCmpConsents))] bool?[] priorNonCmpConsents
    )
    {
        // ARRANGE
        bool showCmpConsentForm = priorNonCmpConsents.Any(x => x is null) || legalAcceptStatus != LegalAcceptStatus.Current;

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer_WithConsents(legalAcceptStatus, priorNonCmpConsents);

        var uiUpdatedTcs = new TaskCompletionSource<bool>();
        void uiUpdatedAction() => uiUpdatedTcs.TrySetResult(true);
        privacyDataProcessorsInitializer.NonCmpConsentRequired.AddListener(uiUpdatedAction);
        privacyDataProcessorsInitializer.NonCmpConsentNotRequired.AddListener(uiUpdatedAction);

        // Need this little local function as awaiting TaskCompletionSource.Task directly always seems to cause a deadlock in Unity
        Task waitForUiUpdated() => uiUpdatedTcs.Task;

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        await waitForUiUpdated();   // UI is now updated but test UI is still waiting for simulated "continue" input

        // Having Toggles pre-checked looks shady, even if their state was already persisted to true, so we always set them to false if the non-CMP dialog is being shown.
        Assert.That(privacyDataProcessorsInitializer.DialogRoot!.gameObject.activeSelf, Is.EqualTo(priorNonCmpConsents.Any(x => x is null) || legalAcceptStatus != LegalAcceptStatus.Current));
        Assert.That(privacyDataProcessorsInitializer.ToggleLegalAccept!.isOn, Is.False);
        Assert.That(privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn, Is.False);
        Assert.That(privacyDataProcessorsInitializer.BtnContinue!.interactable, Is.False);

        if (showCmpConsentForm)
            privacyDataProcessorsInitializer.ToggleLegalAccept.onValueChanged.Invoke(true);

        Assert.That(privacyDataProcessorsInitializer.BtnContinue!.interactable, Is.EqualTo(showCmpConsentForm));

        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        Assert.That(privacyDataProcessorsInitializer.DialogRoot!.gameObject.activeSelf, Is.False);
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ShowsCmpConsentForm(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(getPriorNonCmpConsents))] bool?[] priorNonCmpConsents
    )
    {
        // ARRANGE
        bool showCmpConsentForm = priorNonCmpConsents.Any(x => x is null) || legalAcceptStatus != LegalAcceptStatus.Current;

        string consentStrings = string.Join(",", priorNonCmpConsents.Select(x => x?.ToString() ?? "Null"));
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorNonCmpConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorNonCmpConsents);
        Mock<ITcfCmpAdapter> tcfCmpAdapter = new();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(legalAcceptStatus).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            tcfDataProcessors: [Mock.Of<ITcfDataProcessor>()],
            localPreferences: localPreferences.Object,
            tcfCmpAdapter: tcfCmpAdapter.Object
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        tcfCmpAdapter.Verify(x => x.UpdateConsentInfo(), Times.Once());
        tcfCmpAdapter.Verify(x => x.LoadAndShowConsentFormIfRequiredAsync(), showCmpConsentForm ? Times.Once() : Times.Never());
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_OnlySavesUpdatedNonCmpConsents(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(getPriorNonCmpConsents))] bool?[] priorNonCmpConsents,
        [Values] bool hasNonCmpConsent
    )
    {
        // ARRANGE
        string consentStrings = string.Join(",", priorNonCmpConsents.Select(x => x?.ToString() ?? "Null"));
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorNonCmpConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorNonCmpConsents);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(legalAcceptStatus).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = hasNonCmpConsent;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        for (int index = 0; index < priorNonCmpConsents.Length; index++) {
            string key = nonTcfDataProcessors[index].Object.ConsentPreferenceKey;
            if (priorNonCmpConsents[index] is null)
                localPreferences.Verify(x => x.SetInt(key, hasNonCmpConsent ? 1 : 0), Times.Once());
            else
                localPreferences.Verify(x => x.SetInt(key, It.IsAny<int>()), Times.Never());
        }
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_SetslegalAcceptStatus_IfNotCurrent([Values] LegalAcceptStatus legalAcceptStatus)
    {
        // ARRANGE
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}'...");
        Mock<ILegalAcceptManager> legalAcceptManager = getLegalAcceptManager(legalAcceptStatus);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(legalAcceptManager: legalAcceptManager.Object);

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        legalAcceptManager.Verify(x => x.Accept(), legalAcceptStatus == LegalAcceptStatus.Current ? Times.Never : Times.Once);
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_StartsNonTcfDataCollection_WithCorrectConsents(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(getPriorNonCmpConsents))] bool?[] priorNonCmpConsents,
        [Values] bool hasNonCmpConsent
    )
    {
        // ARRANGE
        string consentStrings = string.Join(",", priorNonCmpConsents.Select(x => x?.ToString() ?? "Null"));
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and consent states are: {consentStrings}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(priorNonCmpConsents.Length);
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorNonCmpConsents);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(legalAcceptStatus).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = hasNonCmpConsent;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        for (int index = 0; index < nonTcfDataProcessors.Length; index++) {
            Mock<INonTcfDataProcessor> nonTcfDataProcessor = nonTcfDataProcessors[index];
            bool? priorConsent = priorNonCmpConsents[index];
            bool shouldBeStarted = priorConsent == true || (priorConsent is null && hasNonCmpConsent);
            nonTcfDataProcessor.Verify(x => x.ToggleDataCollection(true), shouldBeStarted ? Times.Once() : Times.Never());
        }
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_InitializesTcfDataProcessors_Always(
        [Values] LegalAcceptStatus legalAcceptStatus
    )
    {
        // ARRANGE
        Mock<ITcfDataProcessor>[] tcfDataProcessors = [
            new Mock<ITcfDataProcessor>(),
            new Mock<ITcfDataProcessor>()
        ];
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(legalAcceptStatus).Object,
            tcfDataProcessors: [.. tcfDataProcessors.Select(x => x.Object)]
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        foreach (Mock<ITcfDataProcessor> tcfDataProcessor in tcfDataProcessors)
            tcfDataProcessor.Verify(x => x.InitializeAsync(), Times.Once());    // Always initialized, data processor's behavior just depends on loaded TC string internally
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_LogsNotThrows_NonTcfDataCollectionExceptions([Values(0, 1, 2)] int consentCount)
    {
        // ARRANGE
        Exception? loggedException = null;
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptStatus.Current).Object,
            localPreferences: getLocalPreferences(priorConsents: [.. Enumerable.Repeat(true, consentCount)]).Object,
            nonTcfDataProcessors: getNonTcfDataProcessors(consentCount).Select((x, index) => {
                _ = x.Setup(y => y.ToggleDataCollection(true)).Throws(new InvalidOperationException($"AH!! Consent {index} exploded!"));
                return x.Object;
            })
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

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
    public async Task InitializeDataProcessorsWithConsentAsync_LogsNotThrows_TcfUpdateConsentInfoExceptions([Values(0, 1, 2)] int consentCount)
    {
        // ARRANGE
        var tcfCmpAdapter = new Mock<ITcfCmpAdapter>();
        _ = tcfCmpAdapter.Setup(x => x.UpdateConsentInfo()).Throws(new InvalidOperationException("AH!! Updating TCF consent info exploded!"));

        Exception? loggedException = null;
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptStatus.Current).Object,
            tcfCmpAdapter: tcfCmpAdapter.Object,
            tcfDataProcessors: Enumerable.Range(0, consentCount).Select(x => Mock.Of<ITcfDataProcessor>())
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

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
    public async Task InitializeDataProcessorsWithConsentAsync_LogsNotThrows_TcfCmpConsentFormExceptions([Values(0, 1, 2)] int consentCount)
    {
        // ARRANGE
        var tcfCmpAdapter = new Mock<ITcfCmpAdapter>();
        _ = tcfCmpAdapter.Setup(x => x.LoadAndShowConsentFormIfRequiredAsync())
            .ThrowsAsync(new InvalidOperationException("AH!! Loading/showing CMP consent form exploded!"));

        Exception? loggedException = null;
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptStatus.Stale).Object,  // Ensure that dialog is actually shown, so CMP consent form can be shown again too
            tcfCmpAdapter: tcfCmpAdapter.Object,
            tcfDataProcessors: Enumerable.Range(0, consentCount).Select(x => Mock.Of<ITcfDataProcessor>())
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

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
    public async Task InitializeDataProcessorsWithConsentAsync_LogsNotThrows_TcfDataProcessorInitializeExceptions([Values(0, 1, 2)] int consentCount)
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: getLegalAcceptManager(LegalAcceptStatus.Current).Object,
            tcfDataProcessors: tcfDataProcessors
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

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

    [Test]
    public void InitializeDataProcessorsWithConsentAsync_CanBeCanceled()
    {
        // ARRANGE
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer();
        var expectedLegalDocs = new LegalDocument[] { ScriptableObject.CreateInstance<LegalDocument>() };

        // ACT / ASSERT
        _ = Assert.ThrowsAsync<TaskCanceledException>(() =>
            privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync(cancellationToken: new CancellationToken(canceled: true))
        );
    }

    #endregion

    #region WasConsentGranted

    [Test]
    public async Task WasConsentGranted_Throws_NonTcfDataProcessorNotFound()
    {
        // ARRANGE
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = getNonTcfDataProcessor();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(nonTcfDataProcessors: nonTcfDataProcessors);

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        _ = Assert.Throws<ArgumentException>(() => privacyDataProcessorsInitializer.WasConsentGranted(nonTcfDataProcessor.Object));
    }

    [Test]
    public async Task WasConsentGranted_GetsCorrectConent([Values] bool hasConsent)
    {
        // ARRANGE
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = getNonTcfDataProcessors(1);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: getLocalPreferences(priorConsents: [hasConsent]).Object
        );

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        Assert.That(privacyDataProcessorsInitializer.WasConsentGranted(nonTcfDataProcessors[0].Object), Is.EqualTo(hasConsent));
    }

    #endregion

    #region RevokeConsent

    [Test]
    public async Task RevokeConsent_Throws_NonTcfDataProcessorNotFound()
    {
        // ARRANGE
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = getNonTcfDataProcessor();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(nonTcfDataProcessors: nonTcfDataProcessors);

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        _ = Assert.Throws<ArgumentException>(() => privacyDataProcessorsInitializer.RevokeConsent(nonTcfDataProcessor.Object));
    }

    [Test]
    public async Task RevokeConsent_SetsLocalPreference()
    {
        // ARRANGE
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. getNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<ILocalPreferences> localPreferences = getLocalPreferences(priorConsents: [true]);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(LegalAcceptStatus.Current).Object,
            nonTcfDataProcessors: nonTcfDataProcessors,
            localPreferences: localPreferences.Object
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        privacyDataProcessorsInitializer.RevokeConsent(nonTcfDataProcessors[0]);

        // ASSERT
        localPreferences.Verify(x => x.SetInt(nonTcfDataProcessors[0].ConsentPreferenceKey, 0), Times.Once);
    }

    #endregion

#pragma warning restore IDE1006 // Naming Styles

    private static PrivacyDataProcessorsInitializer getPrivacyDataProcessorsInitializer_WithConsents(LegalAcceptStatus legalAcceptStatus, bool?[] priorConsents) =>
        getPrivacyDataProcessorsInitializer(
            legalAcceptManager: getLegalAcceptManager(legalAcceptStatus).Object,
            localPreferences: getLocalPreferences(priorConsents).Object,
            nonTcfDataProcessors: [.. getNonTcfDataProcessors(priorConsents.Length).Select(x => x.Object)]
        );

    private static PrivacyDataProcessorsInitializer getPrivacyDataProcessorsInitializer(
        ILoggerFactory? loggerFactory = null,
        ILegalAcceptManager? legalAcceptManager = null,
        ILocalPreferences? localPreferences = null,
        ITcfCmpAdapter? tcfCmpAdapter = null,
        IEnumerable<ITcfDataProcessor>? tcfDataProcessors = null,
        IEnumerable<INonTcfDataProcessor>? nonTcfDataProcessors = null
    )
    {
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = new GameObject().AddComponent<PrivacyDataProcessorsInitializer>();

        privacyDataProcessorsInitializer.Inject(
            loggerFactory ?? new UnityDebugLoggerFactory(),
            legalAcceptManager ?? getLegalAcceptManager(LegalAcceptStatus.Unprovided).Object,
            localPreferences ?? getLocalPreferences().Object,
            tcfCmpAdapter ?? Mock.Of<ITcfCmpAdapter>(),
            tcfDataProcessors ?? [],
            nonTcfDataProcessors ?? []
        );

        RectTransform dialogRoot = privacyDataProcessorsInitializer.gameObject.AddComponent<RectTransform>();
        privacyDataProcessorsInitializer.DialogRoot = dialogRoot;
        var legalAcceptToggle = new GameObject();
        var nonCmpConsentToggle = new GameObject();
        legalAcceptToggle.transform.parent = dialogRoot;
        nonCmpConsentToggle.transform.parent = dialogRoot;
        privacyDataProcessorsInitializer.ToggleLegalAccept = legalAcceptToggle.AddComponent<Toggle>();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent = nonCmpConsentToggle.AddComponent<Toggle>();
        privacyDataProcessorsInitializer.BtnContinue = privacyDataProcessorsInitializer.gameObject.AddComponent<Button>();

        return privacyDataProcessorsInitializer;
    }

    private static Mock<ILegalAcceptManager> getLegalAcceptManager(LegalAcceptStatus legalAcceptStatus)
    {
        Mock<ILegalAcceptManager> legalAcceptManager = new();
        _ = legalAcceptManager.Setup(x => x.CheckStatusAsync(It.IsAny<IEnumerable<LegalDocument>>())).ReturnsAsync(legalAcceptStatus);
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

    private static void logToUnity(LogLevel logLevel, EventId eventId, Exception? exception, string message)
    {
        LogType logType = logLevel switch {
            LogLevel.None or LogLevel.Trace or LogLevel.Debug or LogLevel.Information => LogType.Log,
            LogLevel.Warning => LogType.Warning,
            LogLevel.Error or LogLevel.Critical => LogType.Error,
            _ => throw UnityObjectExtensions.SwitchDefaultException(logLevel),
        };
        Debug.unityLogger.Log(logType, message);
    }
}
