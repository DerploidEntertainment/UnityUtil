using Microsoft.Extensions.Logging;
using System;
using UnityEngine.Networking;
using UnityUtil.Logging;

namespace UnityUtil.Legal;

/// <inheritdoc/>
internal class LegalLogger<T> : BaseUnityUtilLogger<T>
{
    public LegalLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 6000) { }

    #region Information

    public void LegalAcceptRequired(bool isAcceptOutdated) =>
        LogInformation(id: 0, nameof(LegalAcceptRequired), $"User must accept latest versions of all legal documents {(isAcceptOutdated ? "because the versions that they last accepted are out of date" : "for the first time")}");

    public void LegalAcceptAlreadyAcceptedAll() =>
        LogInformation(id: 1, nameof(LegalAcceptAlreadyAcceptedAll), "User already accepted latest versions of all legal documents");

    public void LegalDocumentAccepted(LegalDocument legalDocument) =>
        LogInformation(id: 2, nameof(LegalDocumentAccepted), "Legal document with latest {header} is now accepted by user and saved to local preferences at {preferencesKey}, so user won't need to accept it again", legalDocument.TagHeader, legalDocument.PreferencesKey);

    public void LegalAcceptCleared() =>
        LogInformation(id: 3, nameof(LegalAcceptCleared), "Accepted tags for all legal documents have been cleared");

    public void ConsentNeedsRequested() =>
        LogInformation(id: 4, nameof(ConsentNeedsRequested), "At least one consent still needs requested. Activating initial consent UI...");

    public void ConsentRequestedLegalDocUpdated() =>
        LogInformation(id: 5, nameof(ConsentRequestedLegalDocUpdated), "All consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance...");

    public void ConsentAlreadyRequested() =>
        LogInformation(id: 6, nameof(ConsentAlreadyRequested), "All consents already requested or not required. Skipping consent UI...");

    public void InitializableConsentAlreadyRequested(string initializableName, DataConsentState dataConsentState) =>
        LogInformation(id: 7, nameof(InitializableConsentAlreadyRequested), $"Consent for {{{nameof(initializableName)}}} already in {{{nameof(dataConsentState)}}}", initializableName, dataConsentState);

    public void InitializableConsentNotRequired(string initializableName) =>
        LogInformation(id: 8, nameof(InitializableConsentNotRequired), $"Consent for {{{nameof(initializableName)}}} does not need need to be requested in the Unity Editor when not forcing consent behavior", initializableName);

    public void InitializableConsentNeedsRequested(string initializableName) =>
        LogInformation(id: 9, nameof(InitializableConsentNeedsRequested), $"Consent for {{{nameof(initializableName)}}} will need to be requested", initializableName);

    public void ConsentGiveAll() =>
        LogInformation(id: 10, nameof(ConsentGiveAll), "Giving data consent to all managers for which consent had not yet been requested...");

    public void InitializableConsentSaving(IInitializableWithConsent initializable) =>
        LogInformation(id: 11, nameof(InitializableConsentSaving), "Saving consent to local preferences at {preferenceKey} so we don't need to request it again...", initializable.ConsentPreferenceKey);

    public void ConsentInitializingAll() =>
        LogInformation(id: 12, nameof(ConsentInitializingAll), "Initializing all data consent managers in parallel...");

    public void ConsentOptingOut(IInitializableWithConsent initializable) =>
        LogInformation(id: 13, nameof(ConsentOptingOut), "Opting out of data consent for initializable with {preferenceKey}. This cannot be undone.", initializable.ConsentPreferenceKey);

    public void ConsentCleared() =>
        LogInformation(id: 14, nameof(ConsentCleared), "Cleared all data consents from preferences");

    #endregion

    #region Warning

    public void LegalDocumentFetchLatestFailed(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        LogWarning(id: 0, nameof(LegalDocumentFetchLatestFailed), "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public void LegalDocumentFetchLatestErrorCode(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        LogWarning(id: 1, nameof(LegalDocumentFetchLatestErrorCode), "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public void LegalDocumentHeaderParseFailedFirstTime(string header, string tag) =>
        LogWarning(id: 2, nameof(LegalDocumentHeaderParseFailedFirstTime), $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. Using random GUID {{{nameof(tag)}}} instead.");

    public void LegalDocumentHeaderParseFailed(string header) =>
        LogWarning(id: 3, nameof(LegalDocumentHeaderParseFailed), $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again.");

    #endregion

    #region Error

    public void ConsentInitializingAllFailed(AggregateException aggregateException) =>
        LogError(id: 0, nameof(ConsentInitializingAllFailed), aggregateException.Flatten(), message: null);

    #endregion

}
