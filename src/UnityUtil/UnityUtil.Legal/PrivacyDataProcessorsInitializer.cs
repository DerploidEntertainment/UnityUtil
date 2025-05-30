using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using UnityUtil.Storage;
using static Microsoft.Extensions.Logging.LogLevel;
using static UnityUtil.Legal.NonCmpConsentStatus;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Legal;

/// <summary>
/// Handles an opinionated user flow to gather user acceptance of legal documents and consent to private data sharing before initializing data processor SDKs.
/// </summary>
/// <remarks>
/// <para>
/// Legal docs that users must accept (such as the Privacy Policy and Terms of Use) are specified in the <see cref="LegalDocuments"/> list.
/// </para>
/// <para>
/// For gathering users' consent to share their private data,
/// data processors are either compliant with the IAB Transparency and Consent Framework (TCF)
/// (<see cref="ITcfDataProcessor"/>s) or not (<see cref="INonTcfDataProcessor"/>s).
/// <list type="bullet">
/// <item>Consent to non-TCF vendors is granted or denied via a Unity UI dialog that app developers must build according to certain specifications (see below).</item>
/// <item>TCF vendors, meanwhile, use the TC string updated by the application's Consent Management Platform (CMP) (represented by <see cref="ITcfCmpAdapter"/>).</item>
/// </list>
/// </para>
/// <para>
/// This class expects the associated Unity UI dialog to have the following:
/// <list type="number">
/// <item>A <see cref="Toggle"/> for the user to accept all <see cref="LegalDocuments"/>.</item>
/// <item>A <see cref="Toggle"/> for the user to grant/deny consent to all registered non-TCF data providers.</item>
/// <item>A <see cref="Toggle"/> for the user to (dis)agree to view the CMP consent form (for personalizing privacy settings with TCF vendors).</item>
/// <item>A <see cref="Button"/> for the user to continue after setting the above toggles.</item>
/// </list>
/// This class handles interactability of those UI elements and determines whether or not to show the dialog at app launch
/// based on the consent/acceptance statuses saved in local preferences (<see cref="ILocalPreferences"/>).
/// </para>
/// </remarks>
public class PrivacyDataProcessorsInitializer : MonoBehaviour
{
    private ILogger<PrivacyDataProcessorsInitializer>? _logger;
    private ILegalAcceptManager? _legalAcceptManager;
    private ILocalPreferences? _localPreferences;
    private ITcfCmpAdapter? _tcfCmpAdapter;
    private List<ITcfDataProcessor>? _tcfDataProcessors;
    private List<INonTcfDataProcessor>? _nonTcfDataProcessors;

    private Task? _preInitializeTask;
    private TaskCompletionSource<bool>? _awaitingContinueTcs;
    private bool _legalAcceptRequired;
    private NonCmpConsentStatus[]? _nonCmpConsentStatuses;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        DialogRoot!.gameObject.SetActive(false);
    }

    public void Inject(
        ILoggerFactory loggerFactory,
        ILegalAcceptManager legalAcceptManager,
        ILocalPreferences localPreferences,
        ITcfCmpAdapter tcfCmpAdapter,
        IEnumerable<ITcfDataProcessor> tcfDataProcessors,
        IEnumerable<INonTcfDataProcessor> nonTcfDataProcessors
    )
    {
        _logger = loggerFactory.CreateLogger(this);
        _legalAcceptManager = legalAcceptManager;
        _localPreferences = localPreferences;
        _tcfCmpAdapter = tcfCmpAdapter;
        _tcfDataProcessors = [.. tcfDataProcessors];
        _nonTcfDataProcessors = [.. nonTcfDataProcessors];
        _nonCmpConsentStatuses = new NonCmpConsentStatus[_nonTcfDataProcessors.Count];
    }


    [Header("UI")]
    [Tooltip($"This transform will be (de)activated to show/hide the single consent/acceptance dialog as necessary.")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public RectTransform? DialogRoot;

    [Tooltip($"Users MUST check this Toggle to accept the {nameof(LegalDocuments)} before the consent buttons are enabled.")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Toggle? ToggleLegalAccept;

    [Tooltip(
        "Users MAY check this Toggle to grant consent to all registered non-TCF vendors (such as Unity Analytics)." +
        "They MAY also leave it unchecked to deny consent to non-TCF vendors."
    )]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Toggle? ToggleNonCmpConsent;

    [Tooltip(
        "Users MAY check this Toggle to show the CMP Consent Form." +
        "They MAY also leave it unchecked to avoid the CMP Consent Form, in which case TCF-compatible data providers MUST be initialized without consent."
    )]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Toggle? ToggleCmpConsent;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Button? BtnContinue;

    [DisableInPlayMode]
    [Tooltip(
        "The legal documents that users must accept before using the application. " +
        "If any of these documents are later updated then an event is raised to prompt users to accept the new version."
    )]
    public LegalDocument[] LegalDocuments = [];

    [Tooltip(
        "Raised when the initial non-CMP consent dialog is necessary; i.e., when consent for any non-TCF data processor has not been saved yet or the legal docs have not been accepted. " +
        "Note that the CMP consent form may also be shown if the application's TCF consent info has been updated."
    )]
    public UnityEvent InitialConsentRequired = new();

    [Tooltip(
        $"Raised when updated legal docs need accepting. If consent for any non-TCF data processor has not been saved yet, then {nameof(InitialConsentRequired)} is raised instead. " +
        "Note that the CMP consent form may also be shown if the application's TCF consent info has been updated."
    )]
    public UnityEvent LegalUpdateRequired = new();

    [Tooltip(
        "Raised when no acceptance UI is necessary; i.e., when the latest legal docs have been accepted, and non-CMP consents have already been saved. " +
        "Note that the CMP consent form may still be shown if the application's TCF consent info has been updated."
    )]
    public UnityEvent NoUiRequired = new();

    /// <summary>
    /// Run the user flow to gather user acceptance of legal documents and consent to private data sharing before initializing data processor SDKs.
    /// The <see cref="InitialConsentRequired"/>, <see cref="LegalUpdateRequired"/>, or <see cref="NoUiRequired"/> events are raised as necessary,
    /// depending on the non-CMP consents and legal document acceptance saved in local preferences.
    /// </summary>
    /// <param name="preInitializeTask">
    /// An optional task that must complete before all data processors are initialized.
    /// This is useful for running actions in the background while the consent dialog is shown:
    /// actions that must still complete before continuing initialization.
    /// These actions could be connecting to databases, retrieving remote configuration, registering service dependencies, etc.
    /// </param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task InitializeDataProcessorsWithConsentAsync(Task? preInitializeTask = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _preInitializeTask = preInitializeTask;

        _awaitingContinueTcs = new TaskCompletionSource<bool>();

        LegalAcceptStatus legalAcceptStatus = await _legalAcceptManager!.CheckStatusAsync(LegalDocuments);

        cancellationToken.ThrowIfCancellationRequested();

        _nonCmpConsentStatuses = [.. _nonTcfDataProcessors!.Select(readNonCmpConsentStatus)];
        bool someNonCmpConsentStillRequired = Array.FindIndex(_nonCmpConsentStatuses, x => x == StillRequired) > -1;

        cancellationToken.ThrowIfCancellationRequested();

        // Initialize non-CMP consent form UI
        DialogRoot!.gameObject.SetActive(someNonCmpConsentStillRequired || legalAcceptStatus != LegalAcceptStatus.Current);
        ToggleLegalAccept!.isOn = legalAcceptStatus == LegalAcceptStatus.Current;
        ToggleNonCmpConsent!.isOn = false;
        ToggleCmpConsent!.isOn = true;  // Always set this Toggle by default, as all it does is show the CMP consent form, from which users ACTUALLY consent
        BtnContinue!.interactable = legalAcceptStatus == LegalAcceptStatus.Current;

        // These UI event handler registrations were originally in Awake(), but then they don't run in Edit Mode tests.
        // So we're kinda changing functionality just to facilitate tests...but whatev, this is the first and only major method of this class that gets called anyway.
        ToggleLegalAccept!.onValueChanged.AddListener(isOn => BtnContinue!.interactable = isOn);
        BtnContinue!.onClick.AddListener(() => _ = _awaitingContinueTcs!.TrySetResult(true));

        // Need this little local function as awaiting TaskCompletionSource.Task directly always seems to cause a deadlock in Unity
        Task uiContinueAsync() => _awaitingContinueTcs!.Task;

        bool hasNonCmpConsent;
        bool showCmpConsent;

        if (someNonCmpConsentStillRequired || legalAcceptStatus == LegalAcceptStatus.Unprovided) {
            _legalAcceptRequired = legalAcceptStatus == LegalAcceptStatus.Unprovided;
            log_NeedsRequested();
            InitialConsentRequired.Invoke();
            await uiContinueAsync();
            hasNonCmpConsent = ToggleNonCmpConsent!.isOn;
            showCmpConsent = ToggleCmpConsent!.isOn;
        }

        else if (legalAcceptStatus == LegalAcceptStatus.Stale) {
            _legalAcceptRequired = true;
            log_RequestedLegalDocUpdated();
            LegalUpdateRequired.Invoke();
            await uiContinueAsync();
            hasNonCmpConsent = ToggleNonCmpConsent!.isOn;
            showCmpConsent = ToggleCmpConsent!.isOn;
        }

        else {
            log_AlreadyRequested();
            NoUiRequired.Invoke();
            hasNonCmpConsent = true;    // No effect; no consent statuses are still required so none will be updated
            showCmpConsent = false;     // Use existing TC string, players can update consent options from an app settings menu
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (_legalAcceptRequired)
            _legalAcceptManager!.Accept();
        // Don't cancel here; saving legal acceptance and non-CMP consents from the first UI dialog should be done together (not rigorously atomic, but close enough)
        saveRequiredNonCmpConsents(hasNonCmpConsent);

        cancellationToken.ThrowIfCancellationRequested();

        // Wait for other pre-initialization actions.
        // These hopefully completed while the consent dialog(s) were shown, but gotta be sure...
        if (_preInitializeTask is not null)
            await _preInitializeTask;

        initializeNonTcfDataProcessors();

        DialogRoot!.gameObject.SetActive(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (_tcfDataProcessors!.Count == 0) {
            log_NoTcfDataProcessors();
            return;
        }

        await updateCmpConsentAsync(showCmpConsent);
        await initializeTcfDataProcessors();    // TCF-compliant data processors are always initialized; they may just be using the "default" TC string
    }

    private async Task updateCmpConsentAsync(bool showCmpConsent)
    {
        try {
            // TC string is always updated, regardless of whether we then show form for users to update consent options
            log_UpdatingCmpConsentInfo();
            _tcfCmpAdapter!.UpdateConsentInfo();
        }
        catch (Exception ex) {
            log_UpdatingCmpConsentInfoFailed(ex);
        }

        if (!showCmpConsent) {
            log_NotShowingCmpConsentForm();
            return;
        }

        try {
            log_ShowingCmpConsentForm();
            await _tcfCmpAdapter!.LoadAndShowConsentFormIfRequiredAsync();
        }
        catch (Exception ex) {
            log_CmpConsentFormFailure(ex);
        }
    }

    private NonCmpConsentStatus readNonCmpConsentStatus(INonTcfDataProcessor nonTcfDataProcessor, int index)
    {
        string name = nonTcfDataProcessor is Component component ? UnityObjectExtensions.GetHierarchyNameWithType(component) : $"non-TCF data processor {index}";

        if (_localPreferences!.HasKey(nonTcfDataProcessor.ConsentPreferenceKey)) {
            NonCmpConsentStatus nonCmpConsentStatus = _localPreferences.GetInt(nonTcfDataProcessor.ConsentPreferenceKey) == 1 ? Granted : Denied;
            log_NonCmpConsentAlreadyRequested(name, nonCmpConsentStatus);
            return nonCmpConsentStatus;
        }

        log_NonCmpConsentNeedsRequested(name);
        return StillRequired;
    }

    /// <summary>
    /// Save consent for all registered <see cref="INonTcfDataProcessor"/>s that did not already have consent saved in local preferences,
    /// and accept the latest legal documents.
    /// </summary>
    private void saveRequiredNonCmpConsents(bool hasConsent)
    {
        NonCmpConsentStatus newConCmpConsentStatus = hasConsent ? Granted : Denied;
        log_SaveAllNonCmpConsents(newConCmpConsentStatus);

        // Store that consents were saved, so we don't request consent again every startup
        int index = 0;
        foreach (INonTcfDataProcessor nonTcfDataProcessor in _nonTcfDataProcessors!) {
            NonCmpConsentStatus nonCmpConsentStatus = _nonCmpConsentStatuses![index];
            if (nonCmpConsentStatus == StillRequired) {
                nonCmpConsentStatus = newConCmpConsentStatus;
                log_SavingNonCmpConsent(nonTcfDataProcessor, nonCmpConsentStatus);
                _localPreferences!.SetInt(nonTcfDataProcessor.ConsentPreferenceKey, hasConsent ? 1 : 0);
            }
            _nonCmpConsentStatuses![index] = nonCmpConsentStatus;
            ++index;
        }
    }

    /// <summary>
    /// Initializes all TCF-registered data processors.
    /// while <see cref="ITcfDataProcessor"/>s are always initialized internally with the application's latest TC string (in parallel, if possible).
    /// </summary>
    /// <exception cref="AggregateException">
    /// One or more data processor initializations calls failed.
    /// See this exception's <see cref="AggregateException.InnerExceptions"/> collection for more details.
    /// </exception>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Async suffix doesn't really look great on async void methods")]
    private async Task initializeTcfDataProcessors()
    {
        log_InitializingAllTcf();

        var tcfTask = Task.WhenAll(
            _tcfDataProcessors!.Select((x, index) => x.InitializeAsync())
        );

        try {
            await tcfTask.ConfigureAwait(true);
        }
        catch {
            log_InitializingTcfFailed(tcfTask.Exception);
        }
    }

    /// <summary>
    /// Initializes all non-TCF-registered data processors.
    /// <see cref="INonTcfDataProcessor"/>s are only initialized if non-CMP consent was granted.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Async suffix doesn't really look great on async void methods")]
    private void initializeNonTcfDataProcessors()
    {
        log_StartingAllNonTcf();

        // In async void methods, exceptions are swallowed by the SynchronizationContext, so just log them and continue.
        // When code awaits a faulted task, only the first exception in the AggregateException is rethrown,
        // so instead we're squashing that exception and logging the more informative AggregateException from the Task object itself.
        // We have to `await` because Unity freezes if we use Task.Wait() for some reason (maybe a deadlock on the Unity thread?).
        try {
            for (int i = 0; i < _nonTcfDataProcessors!.Count; i++) {
                if (_nonCmpConsentStatuses![i] == Granted)
                    _nonTcfDataProcessors![i].StartDataCollection();
            }
        }
        catch (Exception ex) {
            log_InitializingNonTcfFailed(ex);
        }
    }

    /// <summary>
    /// Check if consent was already granted to <paramref name="nonTcfDataProcessor"/>.
    /// </summary>
    /// <param name="nonTcfDataProcessor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Consent has not yet been saved for <paramref name="nonTcfDataProcessor"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="nonTcfDataProcessor"/> is not managed by this class.</exception>
    public bool WasConsentGranted(INonTcfDataProcessor nonTcfDataProcessor)
    {
        int index = _nonTcfDataProcessors!.FindIndex(x => x == nonTcfDataProcessor);
        return index == -1
            ? throw new ArgumentException($"Provided {nameof(nonTcfDataProcessor)} was not in the set provided to this {nameof(PrivacyDataProcessorsInitializer)}", nameof(nonTcfDataProcessor))
            : _nonCmpConsentStatuses![index] == Granted;
    }

    /// <summary>
    /// Revoke consent for <paramref name="nonTcfDataProcessor"/>.
    /// </summary>
    /// <param name="nonTcfDataProcessor"></param>
    /// <exception cref="InvalidOperationException">Consent has not yet been saved for <paramref name="nonTcfDataProcessor"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="nonTcfDataProcessor"/> is not managed by this class.</exception>
    public void RevokeConsent(INonTcfDataProcessor nonTcfDataProcessor)
    {
        int index = _nonTcfDataProcessors!.FindIndex(x => x == nonTcfDataProcessor);
        if (index == -1)
            throw new ArgumentException($"Provided {nameof(nonTcfDataProcessor)} was not in the set provided to this {nameof(PrivacyDataProcessorsInitializer)}", nameof(nonTcfDataProcessor));

        log_RevokingNonCmpConsent(nonTcfDataProcessor);
        _localPreferences!.SetInt(nonTcfDataProcessor.ConsentPreferenceKey, 0);

        try {
            nonTcfDataProcessor.StopDataCollection();
        }
        catch (Exception ex) {
            log_StopNonTcfFailed(ex);
        }
    }

    /// <summary>
    /// Clear non-CMP consents saved in in local preferences for all registered <see cref="INonTcfDataProcessor"/>s.
    /// </summary>
    [Button]
    public void ClearNonCmpConsentPreferences()
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Consent can only be cleared in Play Mode, so that data processor dependencies are registered.");

        foreach (INonTcfDataProcessor nonTcfDataProcessor in _nonTcfDataProcessors!)
            _localPreferences!.DeleteKey(nonTcfDataProcessor.ConsentPreferenceKey);

        // Log success (use debug logger in case this is being run from the Inspector outside Play mode)
        _logger ??= new UnityDebugLoggerFactory().CreateLogger(this);
        log_Cleared();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_NEEDS_REQUESTED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_NeedsRequested)),
        "At least one non-CMP consent still needs requested or legal docs still need accepted. Activating non-CMP consent UI..."
    );
    private void log_NeedsRequested() => LOG_NEEDS_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_RequestedLegalDocUpdated)),
        "All non-CMP consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance..."
    );
    private void log_RequestedLegalDocUpdated() => LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ALREADY_REQUESTED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AlreadyRequested)),
        "All non-CMP consents already saved or not required, and legal docs already accepted. Not activating the non-CMP consent UI..."
    );
    private void log_AlreadyRequested() => LOG_ALREADY_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, NonCmpConsentStatus, Exception?> LOG_NON_CMP_CONSENT_ALREADY_REQUESTED_ACTION = LoggerMessage.Define<string, NonCmpConsentStatus>(Information,
        new EventId(id: 0, nameof(log_NonCmpConsentAlreadyRequested)),
        "Non-CMP consent for non-TCF data processor '{DataProcessor}' already has status {ConsentStatus}"
    );
    private void log_NonCmpConsentAlreadyRequested(string dataProcessorName, NonCmpConsentStatus nonCmpConsentStatus) =>
        LOG_NON_CMP_CONSENT_ALREADY_REQUESTED_ACTION(_logger!, dataProcessorName, nonCmpConsentStatus, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_NON_CMP_CONSENT_NEEDS_REQURESTED_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_NonCmpConsentNeedsRequested)),
        "Non-CMP consent for non-TCF data processor '{DataProcessor}' will need to be requested"
    );
    private void log_NonCmpConsentNeedsRequested(string dataProcessorName) => LOG_NON_CMP_CONSENT_NEEDS_REQURESTED_ACTION(_logger!, dataProcessorName, null);


    private static readonly Action<MEL.ILogger, NonCmpConsentStatus, Exception?> LOG_SAVE_ALL_NON_CMP_ACTION = LoggerMessage.Define<NonCmpConsentStatus>(Information,
        new EventId(id: 0, nameof(log_SaveAllNonCmpConsents)),
        "Saving non-CMP consent to {ConsentStatus} for all non-TCF data processors that did not already have a consent status saved..."
    );
    private void log_SaveAllNonCmpConsents(NonCmpConsentStatus consentStatus) => LOG_SAVE_ALL_NON_CMP_ACTION(_logger!, consentStatus, null);


    private static readonly Action<MEL.ILogger, string, NonCmpConsentStatus, Exception?> LOG_SAVING_NON_CMP_CONSENT_ACTION = LoggerMessage.Define<string, NonCmpConsentStatus>(Information,
        new EventId(id: 0, nameof(log_SavingNonCmpConsent)),
        "Saving non-CMP consent to {ConsentStatus} at local preference key '{PreferenceKey}' so we don't need to request it again..."
    );
    private void log_SavingNonCmpConsent(INonTcfDataProcessor nonTcfDataProcessor, NonCmpConsentStatus consentStatus) =>
        LOG_SAVING_NON_CMP_CONSENT_ACTION(_logger!, nonTcfDataProcessor.ConsentPreferenceKey, consentStatus, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_NO_TCF_VENDORS_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_NoTcfDataProcessors)),
        "No TCF data processors registered, so not calling the CMP"
    );
    private void log_NoTcfDataProcessors() =>
        LOG_NO_TCF_VENDORS_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_UPDATING_CMP_CONSENT_INFO_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_UpdatingCmpConsentInfo)),
        "Updating CMP consent info..."
    );
    private void log_UpdatingCmpConsentInfo() =>
        LOG_UPDATING_CMP_CONSENT_INFO_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_UPDATING_CMP_CONSENT_INFO_FAILED_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_UpdatingCmpConsentInfoFailed)),
        "Updating CMP consent info failed. Continuing, as this should not crash the game."
    );
    private void log_UpdatingCmpConsentInfoFailed(Exception exception) =>
        LOG_UPDATING_CMP_CONSENT_INFO_FAILED_ACTION(_logger!, exception);


    private static readonly Action<MEL.ILogger, Exception?> LOG_SHOWING_CMP_CONSENT_FORM_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_ShowingCmpConsentForm)),
        "Loading/showing the CMP consent form, if required..."
    );
    private void log_ShowingCmpConsentForm() =>
        LOG_SHOWING_CMP_CONSENT_FORM_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_NOT_SHOWING_CMP_CONSENT_FORM_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_NotShowingCmpConsentForm)),
        "Not showing CMP consent form (keeping default TC string) as user did not agree to show it in the non-CMP dialog"
    );
    private void log_NotShowingCmpConsentForm() =>
        LOG_NOT_SHOWING_CMP_CONSENT_FORM_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_CMP_CONSENT_FORM_FAILURE_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_CmpConsentFormFailure)),
        "Loading or showing CMP consent form failed. Continuing, as this should not crash the game."
    );
    private void log_CmpConsentFormFailure(Exception exception) =>
        LOG_CMP_CONSENT_FORM_FAILURE_ACTION(_logger!, exception);


    private static readonly Action<MEL.ILogger, Exception?> LOG_START_ALL_NON_TCF_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_StartingAllNonTcf)),
        "Starting data collection for non-TCF data processors, if they have consent..."
    );
    private void log_StartingAllNonTcf() => LOG_START_ALL_NON_TCF_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_ALL_TCF_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_InitializingAllTcf)),
        "Initializing all TCF vendors (in parallel, if possible)..."
    );
    private void log_InitializingAllTcf() => LOG_INIT_ALL_TCF_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_REVOKING_NON_CMP_CONSENT_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_RevokingNonCmpConsent)),
        "Revoking non-CMP consent for non-TCF data processor with preference key '{PreferenceKey}'. This cannot be undone."
    );
    private void log_RevokingNonCmpConsent(INonTcfDataProcessor nonTcfDataProcessor) => LOG_REVOKING_NON_CMP_CONSENT_ACTION(_logger!, nonTcfDataProcessor.ConsentPreferenceKey, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_STOP_NON_TCF_FAILED_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_StopNonTcfFailed)),
        "Stopping data collection on a non-TCF data processor failed."
    );
    private void log_StopNonTcfFailed(Exception exception) => LOG_STOP_NON_TCF_FAILED_ACTION(_logger!, exception);


    private static readonly Action<MEL.ILogger, Exception?> LOG_CLEARED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_Cleared)),
        "Cleared all non-CMP data consents from preferences"
    );
    private void log_Cleared() => LOG_CLEARED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_NON_TCF_FAILED_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_InitializingNonTcfFailed)),
        $"Failure while initializing non-TCF data processors"
    );

    private void log_InitializingNonTcfFailed(Exception exception) => LOG_INIT_NON_TCF_FAILED_ACTION(_logger!, exception);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_TCF_FAILED_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_InitializingTcfFailed)),
        $"Failure while initializing TCF data processors"
    );

    private void log_InitializingTcfFailed(AggregateException aggregateException) => LOG_INIT_TCF_FAILED_ACTION(_logger!, aggregateException.Flatten());

    #endregion
}
