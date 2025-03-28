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
using UnityUtil.Logging;
using UnityUtil.Storage;

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
        _initializablesWithConsent = initializablesWithConsent.ToList();
        _consents = new (bool isConsentRequired, bool hasConsent)[_initializablesWithConsent.Count];
    }

    [Tooltip(
        "If true, then consent request behavior will play out as if on an end-user device. " +
        "This is useful for testing UI and event-driven logic while in the Editor. " +
        "This flag has no effect when actually running on a device."
    )]
    [field: LabelText(nameof(ForceConsentBehavior)), ShowInInspector]
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

        _consents = _initializablesWithConsent!
            .Select((x, index) => {
                string initializableName = x is Component component ? UnityObjectExtensions.GetHierarchyNameWithType(component) : $"initializable {index}";
                DataConsentState dataConsentState = checkConsent(x, initializableName);
                bool isConsentRequired =
                    dataConsentState == DataConsentState.StillRequired
                    || (dataConsentState == DataConsentState.NotRequired && ForceConsentBehavior);
                bool hasConsent = dataConsentState == DataConsentState.Given;
                return (isConsentRequired, hasConsent);
            })
            .ToArray();

        LegalAcceptance legalAcceptance = await _legalAcceptManager!.CheckAcceptanceAsync();

        if (Array.FindIndex(_consents, x => x.isConsentRequired) > -1 || legalAcceptance == LegalAcceptance.Unprovided) {
            _legalAcceptanceRequired = legalAcceptance == LegalAcceptance.Unprovided;
            _logger!.ConsentNeedsRequested();
            InitialConsentRequired.Invoke();
        }

        else if (legalAcceptance == LegalAcceptance.Stale) {
            _legalAcceptanceRequired = true;
            _logger!.ConsentRequestedLegalDocUpdated();
            LegalUpdateRequired.Invoke();
        }

        else {
            _logger!.ConsentAlreadyRequested();
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
            _logger!.InitializableConsentAlreadyRequested(name, dataConsentState);
            return dataConsentState;
        }

        if (Application.isEditor && !ForceConsentBehavior) {
            _logger!.InitializableConsentNotRequired(name);
            return DataConsentState.NotRequired;
        }

        _logger!.InitializableConsentNeedsRequested(name);
        return DataConsentState.StillRequired;
    }

    /// <summary>
    /// Give consent to all registered <see cref="IInitializableWithConsent"/>s that did not already have consent saved in local preferences,
    /// and accept the latest legal documents.
    /// </summary>
    private void giveConsent()
    {
        _logger!.ConsentGiveAll();

        // Store that consents were given, so we don't request consent again every startup
        int index = 0;
        foreach (IInitializableWithConsent initializableWithConsent in _initializablesWithConsent!) {
            (bool isConsentRequired, bool hasConsent) = _consents![index];
            if (isConsentRequired) {
                _logger!.InitializableConsentSaving(initializableWithConsent);
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
        _logger!.ConsentInitializingAll();

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
            _logger!.ConsentInitializingAllFailed(task.Exception);
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

        _logger!.ConsentOptingOut(initializableWithConsent);
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
        _logger.ConsentCleared();
    }
}
