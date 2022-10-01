using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.Storage;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Legal;

/// <summary>
/// Manages the gathering and persisting of a user's consent to share their personal data with registered <see cref="IInitializableWithConsent"/>s.
/// Consent is expected to be gathered after showing a single UI dialog during the First-Time User Experience (FTUE).
/// </summary>
public class SingleDialogConsentManager : MonoBehaviour, IConsentManager
{
    private ILogger? _logger;
    private ILegalAcceptManager? _legalAcceptManager;
    private ILocalPreferences? _localPreferences;
    private IEnumerable<IInitializableWithConsent>? _initializablesWithConsent;

    private bool _legalAcceptanceRequired;
    private (bool isConsentRequired, bool hasConsent)[]? _consents;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(
        ILoggerProvider loggerProvider,
        ILegalAcceptManager legalAcceptManager,
        ILocalPreferences localPreferences,
        IEnumerable<IInitializableWithConsent> initializablesWithConsent
    )
    {
        _logger = loggerProvider.GetLogger(this);
        _legalAcceptManager = legalAcceptManager;
        _localPreferences = localPreferences;
        _initializablesWithConsent = initializablesWithConsent;
        _consents = new (bool isConsentRequired, bool hasConsent)[_initializablesWithConsent.Count()];
    }

    [Tooltip(
        "If true, then consent request behavior will play out as if on an end-user device. " +
        "This is useful for testing UI and event-driven logic while in the Editor. " +
        "This flag has no effect when actually running on a device."
    )]
    [field: SerializeField, LabelText(nameof(ForceConsentBehavior))]
    public bool ForceConsentBehavior { get; internal set; }

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
    public void ShowDialogIfNeeded()
    {
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

        _legalAcceptManager!.CheckAcceptance(legalAcceptanceCallback);


        void legalAcceptanceCallback(LegalAcceptance legalAcceptance)
        {
            if (Array.FindIndex(_consents, x => x.isConsentRequired) > -1 || legalAcceptance == LegalAcceptance.Unprovided) {
                _legalAcceptanceRequired = legalAcceptance == LegalAcceptance.Unprovided;
                _logger!.Log("At least one consent still needs requested. Activating initial consent UI...", context: this);
                InitialConsentRequired.Invoke();
            }

            else if (legalAcceptance == LegalAcceptance.Stale) {
                _legalAcceptanceRequired = true;
                _logger!.Log("All consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance...", context: this);
                LegalUpdateRequired.Invoke();
            }

            else {
                _logger!.Log("All consents already requested or not required. Skipping consent UI...", context: this);
                NoUiRequired.Invoke();
            }
        }
    }

    private DataConsentState checkConsent(IInitializableWithConsent initializableWithConsent, string name)
    {
        if (_localPreferences!.HasKey(initializableWithConsent.ConsentPreferenceKey)) {
            DataConsentState dataConsentState = _localPreferences.GetInt(initializableWithConsent.ConsentPreferenceKey) == 1 ? DataConsentState.Given : DataConsentState.Denied;
            _logger!.Log($"Consent for {name} already {dataConsentState}.", context: this);
            return dataConsentState;
        }

        if (Application.isEditor && !ForceConsentBehavior) {
            _logger!.Log($"Consent for {name} does not need need to be requested in the Unity Editor when not forcing consent behavior.", context: this);
            return DataConsentState.NotRequired;
        }

        _logger!.Log($"Consent for {name} will need to be requested.", context: this);
        return DataConsentState.StillRequired;
    }

    /// <summary>
    /// Give consent to all registered <see cref="IInitializableWithConsent"/>s that did not already have consent saved in local preferences,
    /// and accept the latest legal documents.
    /// </summary>
    public void GiveConsent()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            // TODO: Request SKAdNetwork permissions on iOS 14.5+
        }

        _logger!.Log($"Giving data consent to all managers for which consent had not yet been requested...", context: this);

        // Store that consents were given, so we don't request consent again every startup
        int index = 0;
        foreach (IInitializableWithConsent initializableWithConsent in _initializablesWithConsent!) {
            (bool isConsentRequired, bool hasConsent) = _consents![index];
            if (isConsentRequired) {
                _logger!.Log($"Saving consent to local preferences at '{initializableWithConsent.ConsentPreferenceKey}' so we don't need to request it again...", context: this);
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
    public async void Initialize()
    {
        _logger!.Log($"Initializing all data consent managers in parallel...", context: this);

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
            _logger!.LogException(task.Exception.Flatten(), context: this);
        }
    }

    public void OptOut(IInitializableWithConsent initializableWithConsent)
    {
        _logger!.Log($"Opting out of data consent for initializable with preferences key '{initializableWithConsent.ConsentPreferenceKey}'. This cannot be undone.", context: this);
        _localPreferences!.SetInt(initializableWithConsent.ConsentPreferenceKey, 0);
    }

    /// <summary>
    /// Clear consent saved in in local preferences for all registered <see cref="IInitializableWithConsent"/>s.
    /// </summary>
    [Button, Conditional("UNITY_EDITOR")]
    public void ClearConsentPreferences()
    {
        Assert.IsTrue(Application.isPlaying, "Consent can only be cleared in Play Mode, so that initializable dependencies are registered.");

        foreach (IInitializableWithConsent initializableWithConsent in _initializablesWithConsent!)
            _localPreferences!.DeleteKey(initializableWithConsent.ConsentPreferenceKey);

        // Log success (use debug logger in case this is being run from the Inspector outside Play mode)
        _logger ??= UnityEngine.Debug.unityLogger;
        _logger.Log($"Cleared all data consents from preferences", context: this);
    }
}
