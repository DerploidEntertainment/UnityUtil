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
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Legal;

/// <summary>
/// Manages the gathering and persisting of a user's consent to share their personal data with registered <see cref="IInitializableWithConsent"/>s.
/// Consent is expected to be gathered after showing a single UI dialog during the First-Time User Experience (FTUE).
/// </summary>
public class SingleDialogConsentManager : MonoBehaviour, IConsentManager
{
    private ILogger<SingleDialogConsentManager>? _logger;
    private ILegalAcceptManager? _legalAcceptManager;
    private ILocalPreferences? _localPreferences;
    private List<IInitializableWithConsent>? _initializablesWithConsent;

    private Task? _preInitializeTask;
    private bool _legalAcceptanceRequired;
    private (bool isConsentRequired, bool hasConsent)[]? _consents;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        BtnInitialConsent!.onClick.AddListener(async () => await ContinueWithConsentAsync());
        BtnLegalUpdate!.onClick.AddListener(async () => await ContinueWithConsentAsync());
    }

    public void Inject(
        ILoggerFactory loggerFactory,
        ILegalAcceptManager legalAcceptManager,
        ILocalPreferences localPreferences,
        IEnumerable<IInitializableWithConsent> initializablesWithConsent
    )
    {
        _logger = loggerFactory.CreateLogger(this);
        _legalAcceptManager = legalAcceptManager;
        _localPreferences = localPreferences;
        _initializablesWithConsent = [.. initializablesWithConsent];
        _consents = new (bool isConsentRequired, bool hasConsent)[_initializablesWithConsent.Count];
    }

    [Tooltip(
        "If true, then consent request behavior will play out as if on an end-user device. " +
        "This is useful for testing UI and event-driven logic while in the Editor. " +
        "This flag has no effect when actually running on a device."
    )]
    [field: LabelText(nameof(ForceConsentBehavior)), ShowInInspector, DisableInPlayMode, SerializeField]
    public bool ForceConsentBehavior { get; internal set; }


    [Header("UI")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Button? BtnInitialConsent;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Button? BtnLegalUpdate;

    [Tooltip("Raised when the initial consent dialog is necessary. I.e., when consents have not been given/denied or the legal docs have not been accepted.")]
    public UnityEvent InitialConsentRequired = new();

    [Tooltip($"Raised when updated legal docs need accepting. If consents have not yet been given/denied, then {nameof(InitialConsentRequired)} is raised instead.")]
    public UnityEvent LegalUpdateRequired = new();

    [Tooltip("Raised when no acceptance UI is necessary. I.e., when the latest legal docs have been accepted, and consents have been given/denied.")]
    public UnityEvent NoUiRequired = new();

    /// <summary>
    /// Raise the <see cref="InitialConsentRequired"/>, <see cref="LegalUpdateRequired"/>, or <see cref="NoUiRequired"/> events as necessary,
    /// depending on the consents and legal documents acceptance saved in local preferences.
    /// </summary>
    /// <param name="preInitializeTask">
    /// An optional task that must complete before all <see cref="IInitializableWithConsent"/>s are initialized.
    /// This is useful for running actions in the background while the consent dialog is shown,
    /// but that must still complete before continuing initialization
    /// (connecting to databases, retrieving remote configuration, registering service dependencies, etc.).
    /// </param>
    public async Task ShowDialogIfNeededAsync(Task? preInitializeTask = null)
    {
        _preInitializeTask = preInitializeTask;

        _consents = [
            .. _initializablesWithConsent!
            .Select((x, index) => {
                string initializableName = x is Component component ? UnityObjectExtensions.GetHierarchyNameWithType(component) : $"initializable {index}";
                DataConsentState dataConsentState = checkConsent(x, initializableName);
                bool isConsentRequired =
                    dataConsentState == DataConsentState.StillRequired
                    || (dataConsentState == DataConsentState.NotRequired && ForceConsentBehavior);
                bool hasConsent = dataConsentState == DataConsentState.Given;
                return (isConsentRequired, hasConsent);
            })
        ];

        LegalAcceptance legalAcceptance = await _legalAcceptManager!.CheckAcceptanceAsync();

        if (Array.FindIndex(_consents, x => x.isConsentRequired) > -1 || legalAcceptance == LegalAcceptance.Unprovided) {
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
            await ContinueWithConsentAsync();
            NoUiRequired.Invoke();
        }
    }

    internal async Task ContinueWithConsentAsync()
    {
        giveConsent();

        // Wait for other pre-initialization actions.
        // These hopefully completed while the consent dialog was shown, but gotta be sure...
        if (_preInitializeTask is not null)
            await _preInitializeTask;

        initialize();
    }

    private DataConsentState checkConsent(IInitializableWithConsent initializableWithConsent, string name)
    {
        if (_localPreferences!.HasKey(initializableWithConsent.ConsentPreferenceKey)) {
            DataConsentState dataConsentState = _localPreferences.GetInt(initializableWithConsent.ConsentPreferenceKey) == 1 ? DataConsentState.Given : DataConsentState.Denied;
            log_InitializableConsentAlreadyRequested(name, dataConsentState);
            return dataConsentState;
        }

        if (Application.isEditor && !ForceConsentBehavior) {
            log_InitializableConsentNotRequired(name);
            return DataConsentState.NotRequired;
        }

        log_InitializableConsentNeedsRequested(name);
        return DataConsentState.StillRequired;
    }

    /// <summary>
    /// Give consent to all registered <see cref="IInitializableWithConsent"/>s that did not already have consent saved in local preferences,
    /// and accept the latest legal documents.
    /// </summary>
    private void giveConsent()
    {
        log_GiveAll();

        // Store that consents were given, so we don't request consent again every startup
        int index = 0;
        foreach (IInitializableWithConsent initializableWithConsent in _initializablesWithConsent!) {
            (bool isConsentRequired, bool hasConsent) = _consents![index];
            if (isConsentRequired) {
                log_InitializableConsentSaving(initializableWithConsent);
                _localPreferences!.SetInt(initializableWithConsent.ConsentPreferenceKey, 1);
                hasConsent = true;
            }
            _consents![index] = (isConsentRequired: false, hasConsent);
            ++index;
        }

        if (_legalAcceptanceRequired)
            _legalAcceptManager!.Accept();
    }

    /// <summary>
    /// Initializes all registered <see cref="IInitializableWithConsent"/>s in parallel, passing them the corresponding flag for consent.
    /// </summary>
    /// <exception cref="AggregateException">
    /// One or more of the <see cref="IInitializableWithConsent.InitializeAsync(bool)"/> calls failed.
    /// See this exception's <see cref="AggregateException.InnerExceptions"/> collection for more details.
    /// </exception>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Async suffix doesn't really look great on async void methods")]
    private async void initialize()
    {
        log_InitializingAll();

        var task = Task.WhenAll(
            _initializablesWithConsent!.Select((x, index) => x.InitializeAsync(_consents![index].hasConsent))
        );

        // In async void methods, exceptions are swallowed by the SynchronizationContext, so just log them and continue.
        // When code awaits a faulted task, only the first exception in the AggregateException is rethrown,
        // so instead we're squashing that exception and logging the more informative AggregateException from the Task object itself.
        // We have to `await` because Unity freezes if we use Task.Wait() for some reason (maybe a deadlock on the Unity thread?).
        try {
            await task.ConfigureAwait(true);
        }
        catch {
            log_InitializingAllFailed(task.Exception);
        }
    }

    public bool HasConsent(IInitializableWithConsent initializableWithConsent)
    {
        int index = _initializablesWithConsent!.FindIndex(x => x == initializableWithConsent);
        return index == -1
            ? throw new ArgumentException($"Provided {nameof(initializableWithConsent)} was not in the set provided to this {nameof(SingleDialogConsentManager)}", nameof(initializableWithConsent))
            : _consents![index].hasConsent;
    }

    public void OptOut(IInitializableWithConsent initializableWithConsent)
    {
        int index = _initializablesWithConsent!.FindIndex(x => x == initializableWithConsent);
        if (index == -1)
            throw new ArgumentException($"Provided {nameof(initializableWithConsent)} was not in the set provided to this {nameof(SingleDialogConsentManager)}", nameof(initializableWithConsent));

        log_OptingOut(initializableWithConsent);
        _localPreferences!.SetInt(initializableWithConsent.ConsentPreferenceKey, 0);
    }

    /// <summary>
    /// Clear consent saved in in local preferences for all registered <see cref="IInitializableWithConsent"/>s.
    /// </summary>
    [Button]
    public void ClearConsentPreferences()
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Consent can only be cleared in Play Mode, so that initializable dependencies are registered.");

        foreach (IInitializableWithConsent initializableWithConsent in _initializablesWithConsent!)
            _localPreferences!.DeleteKey(initializableWithConsent.ConsentPreferenceKey);

        // Log success (use debug logger in case this is being run from the Inspector outside Play mode)
        _logger ??= new UnityDebugLoggerFactory().CreateLogger(this);
        log_Cleared();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_NEEDS_REQUESTED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_NeedsRequested)),
        "At least one consent still needs requested. Activating initial consent UI..."
    );
    private void log_NeedsRequested() => LOG_NEEDS_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_RequestedLegalDocUpdated)),
        "All consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance..."
    );
    private void log_RequestedLegalDocUpdated() => LOG_REQUESTED_LEGAL_DOC_UPDATED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ALREADY_REQUESTED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AlreadyRequested)),
        "All consents already requested or not required. Skipping consent UI..."
    );
    private void log_AlreadyRequested() => LOG_ALREADY_REQUESTED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, DataConsentState, Exception?> LOG_INITIALIZABLE_CONSENT_ALREADY_REQUESTED_ACTION = LoggerMessage.Define<string, DataConsentState>(Information,
        new EventId(id: 0, nameof(log_InitializableConsentAlreadyRequested)),
        "Consent for initializable '{Initializable}' already in state {DataConsentState}"
    );
    private void log_InitializableConsentAlreadyRequested(string initializableName, DataConsentState dataConsentState) =>
        LOG_INITIALIZABLE_CONSENT_ALREADY_REQUESTED_ACTION(_logger!, initializableName, dataConsentState, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_INITIALIZABLE_CONSENT_NOT_REQUIRED_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_InitializableConsentNotRequired)),
        "Consent for initializable '{Initializable}' does not need need to be requested in the Unity Editor when not forcing consent behavior"
    );
    private void log_InitializableConsentNotRequired(string initializableName) => LOG_INITIALIZABLE_CONSENT_NOT_REQUIRED_ACTION(_logger!, initializableName, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_INITIALIZABLE_CONSENT_NEEDS_REQURESTED_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_InitializableConsentNeedsRequested)),
        "Consent for initializable '{Initializable}' will need to be requested"
    );
    private void log_InitializableConsentNeedsRequested(string initializableName) => LOG_INITIALIZABLE_CONSENT_NEEDS_REQURESTED_ACTION(_logger!, initializableName, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_GIVE_ALL_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_GiveAll)),
        "Giving data consent to all managers for which consent had not yet been requested..."
    );
    private void log_GiveAll() => LOG_GIVE_ALL_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_INITIALIZABLE_CONSENT_SAVING_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_InitializableConsentSaving)),
        "Saving consent to local preference key '{PreferenceKey}' so we don't need to request it again..."
    );
    private void log_InitializableConsentSaving(IInitializableWithConsent initializable) =>
        LOG_INITIALIZABLE_CONSENT_SAVING_ACTION(_logger!, initializable.ConsentPreferenceKey, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_ALL_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_InitializingAll)),
        "Initializing all data consent managers in parallel..."
    );
    private void log_InitializingAll() => LOG_INIT_ALL_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_OPTING_OUT_ACTION = LoggerMessage.Define<string>(Information,
        new EventId(id: 0, nameof(log_OptingOut)),
        "Opting out of data consent for initializable with preference key '{PreferenceKey}'. This cannot be undone."
    );
    private void log_OptingOut(IInitializableWithConsent initializable) => LOG_OPTING_OUT_ACTION(_logger!, initializable.ConsentPreferenceKey, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_CLEARED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_Cleared)),
        "Cleared all data consents from preferences"
    );
    private void log_Cleared() => LOG_CLEARED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_INIT_ALL_FAILED_ACTION = LoggerMessage.Define(Error,
        new EventId(id: 0, nameof(log_InitializingAllFailed)),
        $"Initializing all {nameof(IInitializableWithConsent)}s failed"
    );
    private void log_InitializingAllFailed(AggregateException aggregateException) => LOG_INIT_ALL_FAILED_ACTION(_logger!, aggregateException.Flatten());

    #endregion
}
