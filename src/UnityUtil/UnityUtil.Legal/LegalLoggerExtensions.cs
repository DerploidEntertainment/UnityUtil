using System;
using Microsoft.Extensions.Logging;
using UnityEngine.Networking;

namespace UnityUtil.Legal;

/// <inheritdoc/>
internal static class LegalLoggerExtensions
{
    #region Information

    public static void LegalAcceptRequired(this ILogger logger, bool isAcceptOutdated) =>
        logger.LogInformation(new EventId(id: 0, nameof(LegalAcceptRequired)), $"User must accept latest versions of all legal documents {(isAcceptOutdated ? "because the versions that they last accepted are out of date" : "for the first time")}");

    public static void LegalAcceptAlreadyAcceptedAll(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(LegalAcceptAlreadyAcceptedAll)), "User already accepted latest versions of all legal documents");

    public static void LegalDocumentAccepted(this ILogger logger, LegalDocument legalDocument) =>
        logger.LogInformation(new EventId(id: 0, nameof(LegalDocumentAccepted)), "Legal document with latest {header} is now accepted by user and saved to local preferences at {preferencesKey}, so user won't need to accept it again", legalDocument.TagHeader, legalDocument.PreferencesKey);

    public static void LegalAcceptCleared(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(LegalAcceptCleared)), "Accepted tags for all legal documents have been cleared");

    public static void ConsentNeedsRequested(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentNeedsRequested)), "At least one consent still needs requested. Activating initial consent UI...");

    public static void ConsentRequestedLegalDocUpdated(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentRequestedLegalDocUpdated)), "All consents already requested, but at least one legal doc has been updated. Activating UI to get updated legal acceptance...");

    public static void ConsentAlreadyRequested(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentAlreadyRequested)), "All consents already requested or not required. Skipping consent UI...");

    public static void InitializableConsentAlreadyRequested(this ILogger logger, string initializableName, DataConsentState dataConsentState) =>
        logger.LogInformation(new EventId(id: 0, nameof(InitializableConsentAlreadyRequested)), $"Consent for {{{nameof(initializableName)}}} already in {{{nameof(dataConsentState)}}}", initializableName, dataConsentState);

    public static void InitializableConsentNotRequired(this ILogger logger, string initializableName) =>
        logger.LogInformation(new EventId(id: 0, nameof(InitializableConsentNotRequired)), $"Consent for {{{nameof(initializableName)}}} does not need need to be requested in the Unity Editor when not forcing consent behavior", initializableName);

    public static void InitializableConsentNeedsRequested(this ILogger logger, string initializableName) =>
        logger.LogInformation(new EventId(id: 0, nameof(InitializableConsentNeedsRequested)), $"Consent for {{{nameof(initializableName)}}} will need to be requested", initializableName);

    public static void ConsentGiveAll(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentGiveAll)), "Giving data consent to all managers for which consent had not yet been requested...");

    public static void InitializableConsentSaving(this ILogger logger, IInitializableWithConsent initializable) =>
        logger.LogInformation(new EventId(id: 0, nameof(InitializableConsentSaving)), "Saving consent to local preferences at {preferenceKey} so we don't need to request it again...", initializable.ConsentPreferenceKey);

    public static void ConsentInitializingAll(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentInitializingAll)), "Initializing all data consent managers in parallel...");

    public static void ConsentOptingOut(this ILogger logger, IInitializableWithConsent initializable) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentOptingOut)), "Opting out of data consent for initializable with {preferenceKey}. This cannot be undone.", initializable.ConsentPreferenceKey);

    public static void ConsentCleared(this ILogger logger) =>
        logger.LogInformation(new EventId(id: 0, nameof(ConsentCleared)), "Cleared all data consents from preferences");

    #endregion

    #region Warning

    public static void LegalDocumentFetchLatestFailed(this ILogger logger, LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        logger.LogWarning(new EventId(id: 0, nameof(LegalDocumentFetchLatestFailed)), "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public static void LegalDocumentFetchLatestErrorCode(this ILogger logger, LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        logger.LogWarning(new EventId(id: 0, nameof(LegalDocumentFetchLatestErrorCode)), "Unable to fetch latest version of legal document with {uri}. Error received: {error}", legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "");

    public static void LegalDocumentHeaderParseFailedFirstTime(this ILogger logger, string header, string tag) =>
        logger.LogWarning(new EventId(id: 0, nameof(LegalDocumentHeaderParseFailedFirstTime)), $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. Using random GUID {{{nameof(tag)}}} instead.", header, tag);

    public static void LegalDocumentHeaderParseFailed(this ILogger logger, string header) =>
        logger.LogWarning(new EventId(id: 0, nameof(LegalDocumentHeaderParseFailed)), $"Document tag from {{{nameof(header)}}} was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again.", header);

    #endregion

    #region Error

    public static void ConsentInitializingAllFailed(this ILogger logger, AggregateException aggregateException) =>
        logger.LogError(new EventId(id: 0, nameof(ConsentInitializingAllFailed)), aggregateException.Flatten(), message: null);

    #endregion

}
