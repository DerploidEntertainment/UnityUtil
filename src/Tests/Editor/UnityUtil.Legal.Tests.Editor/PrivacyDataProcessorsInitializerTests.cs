using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static UnityUtil.Legal.NonCmpConsentStatus;
using static UnityUtil.Legal.Tests.Editor.PrivacyDataProcessorsInitializerTests.LegalAcceptStatus;

namespace UnityUtil.Legal.Tests.Editor;

public class PrivacyDataProcessorsInitializerTests : BaseEditModeTestFixture
{
    public enum LegalAcceptStatus
    {
        Unprovided,
        Stale,
        Current,
    }

    private const string CONSENT_PREF_KEY_PREFIX = "TEST_CONSENT_";

    private static readonly NonCmpConsentStatus[][] PRIOR_NON_CMP_CONSENTS = [
        [StillRequired],
        [Granted],
        [Denied],
        [StillRequired, StillRequired],
        [StillRequired, Denied],
        [StillRequired, Granted],
        [Denied, StillRequired],
        [Denied, Denied],
        [Denied, Granted],
        [Granted, StillRequired],
        [Granted, Denied],
        [Granted, Granted],
    ];

    private static GameObject? _gameObject;
    private static RectTransform? _dialogRoot;
    private static Toggle? _legalAcceptToggle;
    private static Toggle? _nonCmpConsentToggle;
    private static Button? _btnContinue;
    private static LegalDocument? _legalDocument;

    [SetUp]
    public void SetUp()
    {
        if (_gameObject != null) {
            _legalAcceptToggle!.isOn = false;
            _nonCmpConsentToggle!.isOn = false;
            return;
        }

        _gameObject = new GameObject(nameof(PrivacyDataProcessorsInitializerTests));
        _dialogRoot = _gameObject.AddComponent<RectTransform>();

        _legalAcceptToggle = new GameObject(nameof(PrivacyDataProcessorsInitializer.ToggleLegalAccept)).AddComponent<Toggle>();
        _nonCmpConsentToggle = new GameObject(nameof(PrivacyDataProcessorsInitializer.ToggleNonCmpConsent)).AddComponent<Toggle>();
        _legalAcceptToggle.transform.SetParent(_dialogRoot, worldPositionStays: false);
        _nonCmpConsentToggle.transform.SetParent(_dialogRoot, worldPositionStays: false);

        _btnContinue = _gameObject.AddComponent<Button>();

        _legalDocument = ScriptableObject.CreateInstance<LegalDocument>();
        _legalDocument.name = "legalDoc";
        _legalDocument.PreferencesKey = CONSENT_PREF_KEY_PREFIX + "0";
    }

#pragma warning disable IDE1006 // Naming Styles    // Test names don't need the -Async suffix

    #region InitializeDataProcessorsWithConsentAsync

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ChecksLocalPrefs_ForEachConsent([Values(0, 1, 2)] int consentCount)
    {
        Mock<ILocalPreferences> localPreferences = new();
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = buildNonTcfDataProcessors(consentCount);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
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

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(legalAcceptManager: legalAcceptManager.Object);
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
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses
    )
    {
        // ARRANGE
        int nonCmpConsentRequired_InvokeCount = 0;
        int nonCmpConsentNotRequired_InvokeCount = 0;
        int legalAcceptUnprovided_InvokeCount = 0;
        int legalAcceptStale_InvokeCount = 0;
        int legalAcceptCurrent_InvokeCount = 0;
        bool someNonCmpConsentRequired = priorNonCmpConsentStatuses.Any(x => x == StillRequired);

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer_WithConsents(legalAcceptStatus, priorNonCmpConsentStatuses);
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
        Assert.That(legalAcceptUnprovided_InvokeCount, Is.EqualTo(legalAcceptStatus == Unprovided ? 1 : 0));
        Assert.That(legalAcceptStale_InvokeCount, Is.EqualTo(legalAcceptStatus == Stale ? 1 : 0));
        Assert.That(legalAcceptCurrent_InvokeCount, Is.EqualTo(legalAcceptStatus == Current ? 1 : 0));
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ShowsCorrectUi(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses
    )
    {
        // ARRANGE
        bool showCmpConsentForm = priorNonCmpConsentStatuses.Any(x => x == StillRequired) || legalAcceptStatus != Current;

        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer_WithConsents(legalAcceptStatus, priorNonCmpConsentStatuses);

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
        Assert.That(privacyDataProcessorsInitializer.DialogRoot!.gameObject.activeSelf, Is.EqualTo(priorNonCmpConsentStatuses.Any(x => x == StillRequired) || legalAcceptStatus != Current));
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
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses
    )
    {
        // ARRANGE
        bool showCmpConsentForm = priorNonCmpConsentStatuses.Any(x => x == StillRequired) || legalAcceptStatus != Current;

        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and prior non-CMP consent statuses are: {string.Join(",", priorNonCmpConsentStatuses)}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = buildNonTcfDataProcessors(priorNonCmpConsentStatuses.Length);
        Mock<ILocalPreferences> localPreferences = buildLocalPreferences(priorNonCmpConsentStatuses);
        Mock<ITcfCmpAdapter> tcfCmpAdapter = new();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
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
        tcfCmpAdapter.Verify(x => x.UpdateConsentInfoAsync(), Times.Once());
        tcfCmpAdapter.Verify(x => x.LoadAndShowConsentFormIfRequiredAsync(), Times.Once());
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_ReturnsCorrectStatus(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses,
        [Values(Granted, Denied)] NonCmpConsentStatus nonCmpConsentStatus
    )
    {
        // ARRANGE
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and prior non-CMP consent statuses are: {string.Join(",", priorNonCmpConsentStatuses)}");
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. buildNonTcfDataProcessors(priorNonCmpConsentStatuses.Length).Select(x => x.Object)];
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
            localPreferences: buildLocalPreferences(priorNonCmpConsentStatuses).Object,
            nonTcfDataProcessors: nonTcfDataProcessors
        );

        privacyDataProcessorsInitializer.LegalDocuments = [_legalDocument!];

        bool someNonCmpConsentStillRequired = Array.FindIndex(priorNonCmpConsentStatuses, x => x == StillRequired) > -1;
        bool wasNonCmpConsentFormRequired = someNonCmpConsentStillRequired || legalAcceptStatus != Current;

        var expected = new DataProcessorsStatus(
            legalDocumentStates: privacyDataProcessorsInitializer.LegalDocuments
                .ToDictionary(x => x, x => buildLegalDocumentState(legalAcceptStatus)),
            nonCmpConsentStatuses: Enumerable.Range(0, priorNonCmpConsentStatuses.Length)
                .ToDictionary(x => nonTcfDataProcessors[x], x => wasNonCmpConsentFormRequired ? nonCmpConsentStatus : priorNonCmpConsentStatuses[x])
        );

        // ACT
        Task<DataProcessorsStatus> initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = nonCmpConsentStatus == Granted;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        static string getLegalDocumentStateStr(KeyValuePair<LegalDocument, LegalDocumentState> x) =>
            $"Doc '{x.Key.name}': ('{x.Value.CurrentTag}','{x.Value.AcceptedTag}')";
        Assert.That(initializeTask.Result.LegalDocumentStates, Is.EquivalentTo(expected.LegalDocumentStates),
            $"Expected {nameof(DataProcessorsStatus.LegalDocumentStates)}: {string.Join(", ", expected.LegalDocumentStates.Select(getLegalDocumentStateStr))}\n  " +
            $"Actual {nameof(DataProcessorsStatus.LegalDocumentStates)}: {string.Join(", ", initializeTask.Result.LegalDocumentStates.Select(getLegalDocumentStateStr))}"
        );
        Assert.That(initializeTask.Result.NonCmpConsentStatuses, Is.EquivalentTo(expected.NonCmpConsentStatuses),
            $"Expected {nameof(DataProcessorsStatus.NonCmpConsentStatuses)}: {string.Join(", ", expected.NonCmpConsentStatuses.Select((x, index) => $"Processor {index}:{x.Value}"))}\n  " +
            $"Actual {nameof(DataProcessorsStatus.NonCmpConsentStatuses)}: {string.Join(", ", initializeTask.Result.NonCmpConsentStatuses.Select((x, index) => $"Processor {index}:{x.Value}"))}"
        );
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_OnlySavesUpdatedNonCmpConsents(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses,
        [Values(Granted, Denied)] NonCmpConsentStatus nonCmpConsentStatus
    )
    {
        // ARRANGE
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and prior non-CMP consent statuses are: {string.Join(",", priorNonCmpConsentStatuses)}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = buildNonTcfDataProcessors(priorNonCmpConsentStatuses.Length);
        Mock<ILocalPreferences> localPreferences = buildLocalPreferences(priorNonCmpConsentStatuses);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        bool someNonCmpConsentStillRequired = Array.FindIndex(priorNonCmpConsentStatuses, x => x == StillRequired) > -1;
        bool wasNonCmpConsentFormRequired = someNonCmpConsentStillRequired || legalAcceptStatus != Current;

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = nonCmpConsentStatus == Granted;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        for (int i = 0; i < priorNonCmpConsentStatuses.Length; i++) {
            string key = nonTcfDataProcessors[i].Object.ConsentPreferenceKey;
            localPreferences.Verify(
                x => x.SetInt(key, nonCmpConsentStatus == Granted ? 1 : 0),
                wasNonCmpConsentFormRequired && nonCmpConsentStatus != priorNonCmpConsentStatuses[i] ? Times.Once() : Times.Never()
            );
        }
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_SetslegalAcceptStatus_IfNotCurrent([Values] LegalAcceptStatus legalAcceptStatus)
    {
        // ARRANGE
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}'...");
        Mock<ILegalAcceptManager> legalAcceptManager = buildLegalAcceptManager(legalAcceptStatus);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(legalAcceptManager: legalAcceptManager.Object);

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        legalAcceptManager.Verify(x => x.Accept(), legalAcceptStatus == Current ? Times.Never : Times.Once);
    }

    [Test]
    public async Task InitializeDataProcessorsWithConsentAsync_StartsNonTcfDataCollection_WithCorrectConsents(
        [Values] LegalAcceptStatus legalAcceptStatus,
        [ValueSource(nameof(PRIOR_NON_CMP_CONSENTS))] NonCmpConsentStatus[] priorNonCmpConsentStatuses,
        [Values] NonCmpConsentStatus nonCmpConsentStatus
    )
    {
        // ARRANGE
        Debug.Log($"Initializing data processors when {nameof(legalAcceptStatus)} is '{legalAcceptStatus}' and prior non-CMP consent statuses are: {string.Join(",", priorNonCmpConsentStatuses)}");
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = buildNonTcfDataProcessors(priorNonCmpConsentStatuses.Length);
        Mock<ILocalPreferences> localPreferences = buildLocalPreferences(priorNonCmpConsentStatuses);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: localPreferences.Object
        );

        bool someNonCmpConsentStillRequired = Array.FindIndex(priorNonCmpConsentStatuses, x => x == StillRequired) > -1;
        bool wasNonCmpConsentFormRequired = someNonCmpConsentStillRequired || legalAcceptStatus != Current;

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = nonCmpConsentStatus == Granted;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        // ASSERT
        for (int i = 0; i < nonTcfDataProcessors.Length; i++) {
            bool newHasConsent =
                (wasNonCmpConsentFormRequired && nonCmpConsentStatus == Granted) ||
                (!wasNonCmpConsentFormRequired && priorNonCmpConsentStatuses[i] == Granted);
            nonTcfDataProcessors[i].Verify(x => x.ToggleDataCollection(newHasConsent), Times.Once());
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: buildLegalAcceptManager(Current).Object,
            localPreferences: buildLocalPreferences(nonCmpConsentStatuses: [.. Enumerable.Repeat(Granted, consentCount)]).Object,
            nonTcfDataProcessors: buildNonTcfDataProcessors(consentCount).Select((x, index) => {
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
        _ = tcfCmpAdapter.Setup(x => x.UpdateConsentInfoAsync()).ThrowsAsync(new InvalidOperationException("AH!! Updating TCF consent info exploded!"));

        Exception? loggedException = null;
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: buildLegalAcceptManager(Current).Object,
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: buildLegalAcceptManager(Stale).Object,  // Ensure that dialog is actually shown, so CMP consent form can be shown again too
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            legalAcceptManager: buildLegalAcceptManager(Current).Object,
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
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer();
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
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. buildNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = buildNonTcfDataProcessor();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(nonTcfDataProcessors: nonTcfDataProcessors);

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        _ = Assert.Throws<ArgumentException>(() => privacyDataProcessorsInitializer.WasConsentGranted(nonTcfDataProcessor.Object));
    }

    [Test]
    public async Task WasConsentGranted_GetsCorrectConsent([Values] NonCmpConsentStatus nonCmpConsentStatus)
    {
        // ARRANGE
        Mock<INonTcfDataProcessor>[] nonTcfDataProcessors = buildNonTcfDataProcessors(1);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            nonTcfDataProcessors: [.. nonTcfDataProcessors.Select(x => x.Object)],
            localPreferences: buildLocalPreferences(nonCmpConsentStatuses: [nonCmpConsentStatus]).Object
        );

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = nonCmpConsentStatus == Granted;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        Assert.That(privacyDataProcessorsInitializer.WasConsentGranted(nonTcfDataProcessors[0].Object), Is.EqualTo(nonCmpConsentStatus == Granted));
    }

    #endregion

    #region RevokeConsent

    [Test]
    public async Task ToggleConsent_Throws_NonTcfDataProcessorNotFound([Values] bool hasConsent)
    {
        // ARRANGE
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. buildNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = buildNonTcfDataProcessor();
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(nonTcfDataProcessors: nonTcfDataProcessors);

        // ACT / ASSERT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = hasConsent;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        _ = Assert.Throws<ArgumentException>(() => privacyDataProcessorsInitializer.ToggleConsent(nonTcfDataProcessor.Object, hasConsent));
    }

    [Test]
    public async Task ToggleConsent_TogglesNonTcfDataProcessorCollection([Values] bool hasConsent)
    {
        // ARRANGE
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = buildNonTcfDataProcessor();
        INonTcfDataProcessor[] nonTcfDataProcessors = [nonTcfDataProcessor.Object];
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(nonTcfDataProcessors: nonTcfDataProcessors);

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = !hasConsent;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        privacyDataProcessorsInitializer.ToggleConsent(nonTcfDataProcessor.Object, hasConsent);

        // ASSERT
        nonTcfDataProcessor.Verify(x => x.ToggleDataCollection(hasConsent), Times.Once());
    }

    [Test]
    public async Task ToggleConsent_LogsNotThrows_NonTcfDataProcessorExceptions([Values] NonCmpConsentStatus nonCmpConsentStatus)
    {
        // ARRANGE
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = buildNonTcfDataProcessor();
        _ = nonTcfDataProcessor.SetupSequence(x => x.ToggleDataCollection(It.IsAny<bool>()))
            .Throws(new InvalidOperationException("AH!! Revoke consent exploded!"));
        INonTcfDataProcessor[] nonTcfDataProcessors = [nonTcfDataProcessor.Object];

        Exception? loggedException = null;
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            loggerFactory: new LogLevelCallbackLoggerFactory(LogLevel.Error, (_, _, ex, _) => loggedException = ex, logToUnity),
            localPreferences: buildLocalPreferences([nonCmpConsentStatus]).Object,
            nonTcfDataProcessors: nonTcfDataProcessors
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = nonCmpConsentStatus == Denied;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        privacyDataProcessorsInitializer.ToggleConsent(nonTcfDataProcessor.Object, nonCmpConsentStatus == Granted);

        // ASSERT
        Assert.That(loggedException, Is.Not.Null);
        Assert.That(loggedException, Is.InstanceOf<InvalidOperationException>());
        LogAssert.Expect(LogType.Error, new Regex(".+"));
    }

    [Test]
    public async Task ToggleConsent_SetsLocalPreference_IfConsentChanged(
        [Values] NonCmpConsentStatus initialNonCmpConsentStatus,
        [Values(Granted, Denied)] NonCmpConsentStatus currentNonCmpConsentStatus
    ) {
        // ARRANGE
        INonTcfDataProcessor[] nonTcfDataProcessors = [.. buildNonTcfDataProcessors(1).Select(x => x.Object)];
        Mock<ILocalPreferences> localPreferences = buildLocalPreferences(nonCmpConsentStatuses: [initialNonCmpConsentStatus]);
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(Current).Object,
            nonTcfDataProcessors: nonTcfDataProcessors,
            localPreferences: localPreferences.Object
        );

        // ACT
        Task initializeTask = privacyDataProcessorsInitializer.InitializeDataProcessorsWithConsentAsync();
        privacyDataProcessorsInitializer.ToggleNonCmpConsent!.isOn = initialNonCmpConsentStatus == Granted;
        privacyDataProcessorsInitializer.BtnContinue!.onClick.Invoke();
        await initializeTask;

        privacyDataProcessorsInitializer.ToggleConsent(nonTcfDataProcessors[0], currentNonCmpConsentStatus == Granted);

        // ASSERT
        localPreferences.Verify(
            x => x.SetInt(nonTcfDataProcessors[0].ConsentPreferenceKey, currentNonCmpConsentStatus == Granted ? 1 : 0),
            initialNonCmpConsentStatus == currentNonCmpConsentStatus ? Times.Never() : Times.Once()
        );
    }

    #endregion

#pragma warning restore IDE1006 // Naming Styles

    private static PrivacyDataProcessorsInitializer buildPrivacyDataProcessorsInitializer_WithConsents(LegalAcceptStatus legalAcceptStatus, NonCmpConsentStatus[] priorConsents) =>
        buildPrivacyDataProcessorsInitializer(
            legalAcceptManager: buildLegalAcceptManager(legalAcceptStatus).Object,
            localPreferences: buildLocalPreferences(priorConsents).Object,
            nonTcfDataProcessors: [.. buildNonTcfDataProcessors(priorConsents.Length).Select(x => x.Object)]
        );

    private static PrivacyDataProcessorsInitializer buildPrivacyDataProcessorsInitializer(
        ILoggerFactory? loggerFactory = null,
        ILegalAcceptManager? legalAcceptManager = null,
        ILocalPreferences? localPreferences = null,
        ITcfCmpAdapter? tcfCmpAdapter = null,
        IEnumerable<ITcfDataProcessor>? tcfDataProcessors = null,
        IEnumerable<INonTcfDataProcessor>? nonTcfDataProcessors = null
    )
    {
        PrivacyDataProcessorsInitializer privacyDataProcessorsInitializer = _gameObject!.AddComponent<PrivacyDataProcessorsInitializer>();

        privacyDataProcessorsInitializer.Inject(
            loggerFactory ?? new UnityDebugLoggerFactory(),
            legalAcceptManager ?? buildLegalAcceptManager(Unprovided).Object,
            localPreferences ?? buildLocalPreferences().Object,
            tcfCmpAdapter ?? Mock.Of<ITcfCmpAdapter>(),
            tcfDataProcessors ?? [],
            nonTcfDataProcessors ?? []
        );

        privacyDataProcessorsInitializer.DialogRoot = _dialogRoot;
        privacyDataProcessorsInitializer.LegalDocuments = [_legalDocument!];
        privacyDataProcessorsInitializer.ToggleLegalAccept = _legalAcceptToggle;
        privacyDataProcessorsInitializer.ToggleNonCmpConsent = _nonCmpConsentToggle;
        privacyDataProcessorsInitializer.BtnContinue = _btnContinue;

        return privacyDataProcessorsInitializer;
    }

    private static Mock<ILegalAcceptManager> buildLegalAcceptManager(LegalAcceptStatus legalAcceptStatus)
    {
        Mock<ILegalAcceptManager> legalAcceptManager = new();
        _ = legalAcceptManager.Setup(x => x.CheckStatusAsync(It.IsAny<IEnumerable<LegalDocument>>()))
            .ReturnsAsync((IEnumerable<LegalDocument> docs) =>
                docs.Select(x => buildLegalDocumentState(legalAcceptStatus))
            );
        return legalAcceptManager;
    }

    private static LegalDocumentState buildLegalDocumentState(LegalAcceptStatus legalAcceptStatus)
    {
        const string LEGAL_TAG_CURRENT = "CurrentTagValue";
        const string LEGAL_TAG_STALE = "StaleTagValue";
        return legalAcceptStatus switch {
            Unprovided => new(currentTag: LEGAL_TAG_CURRENT, acceptedTag: null),
            Stale => new(currentTag: LEGAL_TAG_CURRENT, acceptedTag: LEGAL_TAG_STALE),
            Current => new(currentTag: LEGAL_TAG_CURRENT, acceptedTag: LEGAL_TAG_CURRENT),
            _ => throw new InvalidEnumArgumentException(nameof(legalAcceptStatus), (int)legalAcceptStatus, typeof(LegalAcceptStatus)),
        };
    }

    private static Mock<ILocalPreferences> buildLocalPreferences(NonCmpConsentStatus[]? nonCmpConsentStatuses = null)
    {
        Mock<ILocalPreferences> localPreferences = new();
        if (nonCmpConsentStatuses is null)
            return localPreferences;

        for (int x = 0; x < nonCmpConsentStatuses.Length; x++) {
            NonCmpConsentStatus nonCmpConsentStatus = nonCmpConsentStatuses[x];
            string key = $"{CONSENT_PREF_KEY_PREFIX}{x}";
            _ = localPreferences.Setup(x => x.HasKey(key)).Returns(nonCmpConsentStatus != StillRequired);
            if (nonCmpConsentStatus != StillRequired)
                _ = localPreferences.Setup(x => x.GetInt(key)).Returns(nonCmpConsentStatus == Granted ? 1 : 0);
        }

        return localPreferences;
    }

    private static Mock<INonTcfDataProcessor>[] buildNonTcfDataProcessors(int consentCount) =>
        [.. Enumerable
            .Range(0, consentCount)
            .Select(x => {
                Mock<INonTcfDataProcessor> nonTcfDataProcessor = new();
                _ = nonTcfDataProcessor.SetupGet(x => x.ConsentPreferenceKey).Returns($"{CONSENT_PREF_KEY_PREFIX}{x}");
                return nonTcfDataProcessor;
            })
        ];

    private static Mock<INonTcfDataProcessor> buildNonTcfDataProcessor()
    {
        Mock<INonTcfDataProcessor> nonTcfDataProcessor = new();
        _ = nonTcfDataProcessor.SetupGet(x => x.ConsentPreferenceKey).Returns($"{CONSENT_PREF_KEY_PREFIX}0");
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
        Debug.unityLogger.Log(logType, $"{logType}: {message}");
    }
}
