using Microsoft.Extensions.Logging;
using System;
using UnityEngine.Networking;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Legal;

/// <inheritdoc/>
internal class LegalLogger<T> : BaseUnityUtilLogger<T>
{
    public LegalLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 6000) { }

    #region Information

    public void LegalAcceptRequired(bool isAcceptOutdated) =>
        Log(id: 0, nameof(LegalAcceptRequired), Information, $"User must accept latest versions of all legal documents {(isAcceptOutdated ? "because the versions that they last accepted are out of date" : "for the first time")}");

    public void LegalAcceptAlreadyAcceptedAll() =>
        Log(id: 1, nameof(LegalAcceptAlreadyAcceptedAll), Information, "User already accepted latest versions of all legal documents");

    public void LegalDocumentAccepted(LegalDocument legalDocument) =>
        Log(id: 2, nameof(LegalDocumentAccepted), Information, "Legal document with latest {header} is now accepted by user and saved to local preferences at {preferencesKey}, so user won't need to accept it again", legalDocument.TagHeader, legalDocument.PreferencesKey);

    public void LegalAcceptCleared() =>
        Log(id: 3, nameof(LegalAcceptCleared), Information, "Accepted tags for all legal documents have been cleared");

    public void ConsentNeedsRequested() =>
        Log(id: 4, nameof(ConsentNeedsRequested), Information, "At least one consent still needs requested. Activating initial consent UI...");

    public void ConsentRequestedLegalDocUpdated() =>
        Log(id: 5, nameof(ConsentRequestedLegalDocUpdated), Information, "All consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance...");

    public void ConsentAlreadyRequested() =>
        Log(id: 6, nameof(ConsentAlreadyRequested), Information, "All consents already requested or not required. Skipping consent UI...");

    public void InitializableConsentAlreadyRequested(string initializableName, DataConsentState dataConsentState) =>
        Log(id: 7, nameof(InitializableConsentAlreadyRequested), Information, $"Consent for {{{nameof(initializableName)}}} already in {{{nameof(dataConsentState)}}}", initializableName, dataConsentState);

    public void InitializableConsentNotRequired(string initializableName) =>
        Log(id: 8, nameof(InitializableConsentNotRequired), Information, $"Consent for {{{nameof(initializableName)}}} does not need need to be requested in the Unity Editor when not forcing consent behavior", initializableName);

    public void InitializableConsentNeedsRequested(string initializableName) =>
        Log(id: 9, nameof(InitializableConsentNeedsRequested), Information, $"Consent for {{{nameof(initializableName)}}} will need to be requested", initializableName);

    public void ConsentGiveAll() =>
        Log(id: 10, nameof(ConsentGiveAll), Information, "Giving data consent to all managers for which consent had not yet been requested...");

    public void InitializableConsentSaving(IInitializableWithConsent initializable) =>
        Log(id: 11, nameof(InitializableConsentSaving), Information, "Saving consent to local preferences at {preferenceKey} so we don't need to request it again...", initializable.ConsentPreferenceKey);

    public void ConsentInitializingAll() =>
        Log(id: 12, nameof(ConsentInitializingAll), Information, "Initializing all data consent managers in parallel...");

    public void ConsentOptingOut(IInitializableWithConsent initializable) =>
        Log(id: 13, nameof(ConsentOptingOut), Information, "Opting out of data consent for initializable with {preferenceKey}. This cannot be undone.", initializable.ConsentPreferenceKey);

    public void ConsentCleared() =>
        Log(id: 14, nameof(ConsentCleared), Information, "Cleared all data consents from preferences");

    #endregion

    #region Warning

    public void LegalDocumentFetchLatesetFailed(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        Log(id: 0, nameof(LegalDocumentFetchLatesetFailed), Warning, "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public void LegalDocumentFetchLatesetErrorCode(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        Log(id: 1, nameof(LegalDocumentFetchLatesetErrorCode), Warning, "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public void LegalDocumentHeaderParseFailedFirstTime(string header, string tag) =>
        Log(id: 2, nameof(LegalDocumentHeaderParseFailedFirstTime), Warning, $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. Using random GUID {{{nameof(tag)}}} instead.");

    public void LegalDocumentHeaderParseFailed(string header) =>
        Log(id: 3, nameof(LegalDocumentHeaderParseFailed), Warning, $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again.");

    #endregion

    #region Error

    public void ConsentInitializingAllFailed(AggregateException aggregateException) =>
        Log(id: 0, nameof(ConsentInitializingAllFailed), Error, aggregateException.Flatten(), message: null);

    #endregion

}
