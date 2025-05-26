using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

/// <inheritdoc cref="IConsentManager" />
/// <remarks>
/// Acceptance of legal docs and granting consent for non-TCF-compliant processors
/// are both provided through a single UI dialog during the First-Time User Experience (FTUE),
/// followed by a Consent Management Platform (CMP) form for TCF-compliant processors.
/// Consent for each data non-TCF data processor (grant or deny) is saved in local preferences.
/// </remarks>
public class SingleDialogConsentManager : MonoBehaviour, IConsentManager
{
    private ILogger<SingleDialogConsentManager>? _logger;
    private ILegalAcceptManager? _legalAcceptManager;
    private ILocalPreferences? _localPreferences;
    private ITcfCmpAdapter? _tcfCmpAdapter;
    private List<ITcfDataProcessor>? _tcfDataProcessors;
    private List<INonTcfDataProcessor>? _nonTcfDataProcessors;

    private Task? _preInitializeTask;
    private bool _legalAcceptanceRequired;
    private NonCmpConsentStatus[]? _consentStatuses;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        BtnInitialConsent!.onClick.AddListener(async () => await ContinueWithNonCmpConsentAsync());
        BtnLegalUpdate!.onClick.AddListener(async () => await ContinueWithNonCmpConsentAsync());
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
        _consentStatuses = new NonCmpConsentStatus[_nonTcfDataProcessors.Count];
    }


    [Header("UI")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Button? BtnInitialConsent;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Button? BtnLegalUpdate;

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
    /// Raise the <see cref="InitialConsentRequired"/>, <see cref="LegalUpdateRequired"/>, or <see cref="NoUiRequired"/> events as necessary,
    /// depending on the non-CMP consents and legal documents acceptance saved in local preferences.
    /// </summary>
    /// <param name="preInitializeTask">
    /// An optional task that must complete before all data processors are initialized.
    /// This is useful for running actions in the background while the consent dialog is shown,
    /// but that must still complete before continuing initialization
    /// (connecting to databases, retrieving remote configuration, registering service dependencies, etc.).
    /// </param>
    public async Task ShowDialogIfNeededAsync(Task? preInitializeTask = null)
    {
        _preInitializeTask = preInitializeTask;

        _consentStatuses = [
            .. _nonTcfDataProcessors!.Select((x, index) => {
                string dataProcessorName = x is Component component ? UnityObjectExtensions.GetHierarchyNameWithType(component) : $"non-TCF data processor {index}";
                return checkConsent(x, dataProcessorName);
            })
        ];

        LegalAcceptance legalAcceptance = await _legalAcceptManager!.CheckAcceptanceAsync();

        if (Array.FindIndex(_consentStatuses, x => x == StillRequired) > -1 || legalAcceptance == LegalAcceptance.Unprovided) {
            _legalAcceptanceRequired = legalAcceptance == LegalAcceptance.Unprovided;
            log_NeedsRequested();
            InitialConsentRequired.Invoke();
        }

        else if (legalAcceptance == LegalAcceptance.Stale) {
            _legalAcceptanceRequired = true;
            log_RequestedLegalDocUpdated();
            LegalUpdateRequired.Invoke();
        }

        else {
            log_AlreadyRequested();
            await ContinueWithNonCmpConsentAsync();
            NoUiRequired.Invoke();
        }
    }

    internal async Task ContinueWithNonCmpConsentAsync()
    {
        saveNonCmpConsent();

        if (_legalAcceptanceRequired)
            _legalAcceptManager!.Accept();

        log_ShowingCmpConsentForm();
        await _tcfCmpAdapter!.LoadAndShowConsentFormIfRequiredAsync();

        // Wait for other pre-initialization actions.
        // These hopefully completed while the consent dialog(s) were shown, but gotta be sure...
        if (_preInitializeTask is not null)
            await _preInitializeTask;

        initializeDataProcessors();
    }

    private NonCmpConsentStatus checkConsent(INonTcfDataProcessor nonTcfDataProcessor, string name)
    {
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
    private void saveNonCmpConsent()
    {
        log_SaveAll();

        // Store that consents were saved, so we don't request consent again every startup
        int index = 0;
        foreach (INonTcfDataProcessor nonTcfDataProcessor in _nonTcfDataProcessors!) {
            NonCmpConsentStatus nonCmpConsentStatus = _consentStatuses![index];
            if (nonCmpConsentStatus == StillRequired) {
                log_SavingNonCmpConsent(nonTcfDataProcessor);
                _localPreferences!.SetInt(nonTcfDataProcessor.ConsentPreferenceKey, 1);
                nonCmpConsentStatus = Granted;
            }
            _consentStatuses![index] = nonCmpConsentStatus;
            ++index;
        }
    }

    /// <summary>
    /// Initializes all registered data processors.
    /// <see cref="INonTcfDataProcessor"/>s are only initialized if non-CMP consent was granted,
    /// while <see cref="ITcfDataProcessor"/>s are always initialized internally with the application's latest TC string.
    /// (in parallel, if possible), passing them the corresponding flag for consent.
    /// </summary>
    /// <exception cref="AggregateException">
    /// One or more data processor initializations calls failed.
    /// See this exception's <see cref="AggregateException.InnerExceptions"/> collection for more details.
    /// </exception>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Async suffix doesn't really look great on async void methods")]
    private async void initializeDataProcessors()
    {
        log_InitializingAll();

        try {
            for (int i = 0; i < _nonTcfDataProcessors!.Count; i++) {
                if (_consentStatuses![i] == Granted)
                    _nonTcfDataProcessors![i].StartDataCollection();
            }
        }
        catch (Exception ex) {
            log_InitializingNonTcfFailed(ex);
        }

        var tcfTask = Task.WhenAll(
            _tcfDataProcessors!.Select((x, index) => x.InitializeAsync())
        );

        // In async void methods, exceptions are swallowed by the SynchronizationContext, so just log them and continue.
        // When code awaits a faulted task, only the first exception in the AggregateException is rethrown,
        // so instead we're squashing that exception and logging the more informative AggregateException from the Task object itself.
        // We have to `await` because Unity freezes if we use Task.Wait() for some reason (maybe a deadlock on the Unity thread?).
        try {
            await tcfTask.ConfigureAwait(true);
        }
        catch {
            log_InitializingTcfFailed(tcfTask.Exception);
        }
    }

    /// <inheritdoc/>
    public bool WasConsentGranted(INonTcfDataProcessor nonTcfDataProcessor)
    {
        int index = _nonTcfDataProcessors!.FindIndex(x => x == nonTcfDataProcessor);
        return index == -1
            ? throw new ArgumentException($"Provided {nameof(nonTcfDataProcessor)} was not in the set provided to this {nameof(SingleDialogConsentManager)}", nameof(nonTcfDataProcessor))
            : _consentStatuses![index] == Granted;
    }

    /// <inheritdoc/>
    public void RevokeConsent(INonTcfDataProcessor nonTcfDataProcessor)
    {
        int index = _nonTcfDataProcessors!.FindIndex(x => x == nonTcfDataProcessor);
        if (index == -1)
            throw new ArgumentException($"Provided {nameof(nonTcfDataProcessor)} was not in the set provided to this {nameof(SingleDialogConsentManager)}", nameof(nonTcfDataProcessor));

        log_RevokingNonCmpConsent(nonTcfDataProcessor);
        _localPreferences!.SetInt(nonTcfDataProcessor.ConsentPreferenceKey, 0);
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
        "At least one non-CMP consent still needs requested. Activating initial consent UI..."
    );
    private void log_NeedsRequested() => LOG_NEEDS_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_RequestedLegalDocUpdated)),
        "All non-CMP consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance..."
    );
    private void log_RequestedLegalDocUpdated() => LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ALREADY_REQUESTED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AlreadyRequested)),
        "All non-CMP consents already requested or not required. Skipping non-CMP consent UI..."
    );
    private void log_AlreadyRequested() => LOG_ALREADY_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, NonCmpConsentStatus, Exception?> LOG_NON_CMP_CONSENT_ALREADY_REQUESTED_ACTION = LoggerMessage.Define<string, NonCmpConsentStatus>(Information,
        new EventId(id: 0, nameof(log_NonCmpConsentAlreadyRequested)),
        "Non-CMP consent for non-TCF data processor '{DataProcessor}' already has status {ConsentStatus}"
    );
    private void log_NonCmpConsentAlreadyRequested(string dataProcessorName, NonCmpConsentStatus nonCmpConsentStatus) =>
        LOG_NON_CMP_CONSENT_ALREADY_REQUESTED_ACTION(_logger!, dataProcessorName, nonCmpConsentStatus, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_NON_CMP_CONSENT_NOT_REQUIRED_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_NonCmpConsentNotRequired)),
        "Non-CMP consent for non-TCF data processor '{DataProcessor}' does not need need to be requested in the Unity Editor when not forcing consent behavior"
    );
    private void log_NonCmpConsentNotRequired(string dataProcessorName) => LOG_NON_CMP_CONSENT_NOT_REQUIRED_ACTION(_logger!, dataProcessorName, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_NON_CMP_CONSENT_NEEDS_REQURESTED_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_NonCmpConsentNeedsRequested)),
        "Non-CMP consent for non-TCF data processor '{DataProcessor}' will need to be requested"
    );
    private void log_NonCmpConsentNeedsRequested(string dataProcessorName) => LOG_NON_CMP_CONSENT_NEEDS_REQURESTED_ACTION(_logger!, dataProcessorName, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_SAVE_ALL_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_SaveAll)),
        "Saving non-CMP consent for all non-TCF data processors for which consent had not yet been requested..."
    );
    private void log_SaveAll() => LOG_SAVE_ALL_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_SAVING_NON_CMP_CONSENT_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_SavingNonCmpConsent)),
        "Saving non-CMP consent to local preference key '{PreferenceKey}' so we don't need to request it again..."
    );
    private void log_SavingNonCmpConsent(INonTcfDataProcessor nonTcfDataProcessor) =>
        LOG_SAVING_NON_CMP_CONSENT_ACTION(_logger!, nonTcfDataProcessor.ConsentPreferenceKey, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_SHOWING_CMP_CONSENT_FORM_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_ShowingCmpConsentForm)),
        "Loading/showing the CMP consent form, if required..."
    );
    private void log_ShowingCmpConsentForm() =>
        LOG_SHOWING_CMP_CONSENT_FORM_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_ALL_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_InitializingAll)),
        "Initializing all data processors (in parallel, if possible)..."
    );
    private void log_InitializingAll() => LOG_INIT_ALL_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_REVOKING_NON_CMP_CONSENT_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_RevokingNonCmpConsent)),
        "Revoking non-CMP consent for non-TCF data processor with preference key '{PreferenceKey}'. This cannot be undone."
    );
    private void log_RevokingNonCmpConsent(INonTcfDataProcessor nonTcfDataProcessor) => LOG_REVOKING_NON_CMP_CONSENT_ACTION(_logger!, nonTcfDataProcessor.ConsentPreferenceKey, null);


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
