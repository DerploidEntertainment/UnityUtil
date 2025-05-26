using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using UnityUtil.Storage;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Legal;
internal class LegalDocumentState(string currentTag)
{
    public string CurrentTag = currentTag;
    public string? AcceptedTag;
}

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(Legal)}/{nameof(LegalAcceptManager)}", fileName = "legal-accept-manager")]
public class LegalAcceptManager : ScriptableObject, ILegalAcceptManager
{
    private ILogger<LegalAcceptManager>? _logger;
    private ILocalPreferences? _localPreferences;

    private DownloadHandler? _downloadHandler;
    private UploadHandler? _uploadHandler;

    private string[] _latestVersionTags = [];

    [DisableInPlayMode]
    public LegalDocument[] Documents = [];

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory, ILocalPreferences localPreferences)
    {
        _logger = loggerFactory.CreateLogger(this);
        _localPreferences = localPreferences;
    }

    public void SetHandlers(DownloadHandler downloadHandler, UploadHandler uploadHandler)
    {
        _downloadHandler = downloadHandler;
        _uploadHandler = uploadHandler;
    }

    public async Task<LegalAcceptance> CheckAcceptanceAsync()
    {
        LegalDocumentState[] legalDocumentStates = await Task.WhenAll(
            Enumerable.Range(0, Documents.Length).Select(CheckForUpdateAsync)
        );

        // If the tags from the web do not match the accepted tags in preferences, then
        // return "unprovided" or "stale" state, depending on whether a tag existed in preferences at all
        bool acceptRequired = false;
        bool acceptOutdated = false;
        _latestVersionTags = new string[Documents.Length];
        for (int x = 0; x < legalDocumentStates.Length; x++) {
            LegalDocumentState legalDocumentState = legalDocumentStates[x];
            acceptRequired |= legalDocumentState.CurrentTag != legalDocumentState.AcceptedTag;
            acceptOutdated |= acceptRequired && !string.IsNullOrEmpty(legalDocumentState.AcceptedTag);
            _latestVersionTags[x] = legalDocumentState.CurrentTag;
        }

        if (acceptRequired) {
            if (acceptOutdated)
                log_AcceptRequired_OutOfDate();
            else
                log_AcceptRequired_FirstTime();
            return acceptOutdated ? LegalAcceptance.Stale : LegalAcceptance.Unprovided;
        }
        else {
            log_AlreadyAcceptedAll();
            return LegalAcceptance.Current;
        }
    }

    internal Task<LegalDocumentState> CheckForUpdateAsync(int documentIndex)
    {
        var tcs = new TaskCompletionSource<LegalDocumentState>();

        // Get the last accepted tag from preferences (will be empty if none stored yet)
        LegalDocument doc = Documents[documentIndex];
        string acceptedTag = _localPreferences!.GetString(doc.PreferencesKey);

        UnityWebRequest? req = null;    // Must not be disposed until after we get response values in callback below
        try {
            // Get the latest tag from the web
            req = _downloadHandler is null && _uploadHandler is null
                ? new UnityWebRequest(doc.LatestVersionUri!.Uri, UnityWebRequest.kHttpVerbHEAD)
                : new UnityWebRequest(doc.LatestVersionUri!.Uri, UnityWebRequest.kHttpVerbHEAD, _downloadHandler, _uploadHandler);
            UnityWebRequestAsyncOperation reqOp = req.SendWebRequest();
            reqOp.completed += _ => onRequestCompleted();
        }
        catch (Exception ex) {
            log_FetchLatestFailed(doc, req);
            req?.Dispose();
            tcs.SetException(ex);
        }

        return tcs.Task;


        void onRequestCompleted()
        {
            // Parse the tag from the response
            string? currentTag = null;
            if (req.result != UnityWebRequest.Result.Success)
                log_FetchLatestErrorCode(doc, req);
            else
                currentTag = req.GetResponseHeader(doc.TagHeader);

            req!.Dispose();

            // If unable to parse tag due to network or server errors, then
            // use a random GUID as the tag (shouldn't collide with an existing accepted tag) or the last accepted tag if one exists
            if (string.IsNullOrEmpty(currentTag)) {
                if (string.IsNullOrEmpty(acceptedTag)) {
                    currentTag = Guid.NewGuid().ToString();
                    log_HeaderParseFailedFirstTime(doc.TagHeader, currentTag);
                }
                else {
                    currentTag = acceptedTag;
                    log_HeaderParseFailed(doc.TagHeader);
                }
            }

            tcs.SetResult(
                new LegalDocumentState(currentTag) {
                    AcceptedTag = string.IsNullOrEmpty(acceptedTag) ? null : acceptedTag,
                }
            );
        }
    }
    public bool HasAccepted { get; private set; }

    public void Accept()
    {
        for (int v = 0; v < _latestVersionTags.Length; ++v) {
            _localPreferences!.SetString(Documents[v].PreferencesKey, _latestVersionTags[v].ToString());
            log_DocumentAccepted(Documents[v]);
        }

        HasAccepted = true;

    }

    [Button, Conditional("DEBUG")]
    public void ClearAcceptance()
    {
        // Use PlayerPrefs in case this is being run from the Inspector outside Play mode
        if (_localPreferences == null) {
            for (int d = 0; d < Documents.Length; ++d)
                PlayerPrefs.DeleteKey(Documents[d].PreferencesKey);
        }

        else {
            for (int d = 0; d < Documents.Length; ++d)
                _localPreferences.DeleteKey(Documents[d].PreferencesKey);
        }

        _logger ??= new UnityDebugLoggerFactory().CreateLogger(this);    // Use debug logger in case this is being run from the Inspector outside Play mode
        log_AcceptCleared();
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, Exception?> LOG_ACCEPT_REQUIRED_1ST_TIME_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AcceptRequired_FirstTime)),
        $"User must accept latest versions of all legal documents for the first time"
    );
    private void log_AcceptRequired_FirstTime() => LOG_ACCEPT_REQUIRED_1ST_TIME_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ACCEPT_REQUIRED_OUT_OF_DATE_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AcceptRequired_OutOfDate)),
        $"User must accept latest versions of all legal documents because the versions that they last accepted are out of date"
    );
    private void log_AcceptRequired_OutOfDate() => LOG_ACCEPT_REQUIRED_OUT_OF_DATE_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ALREADY_ACCEPTED_ALL_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AlreadyAcceptedAll)),
        "User already accepted latest versions of all legal documents"
    );
    private void log_AlreadyAcceptedAll() => LOG_ALREADY_ACCEPTED_ALL_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, string, Exception?> LOG_DOC_ACCEPTED_ACTION = LoggerMessage.Define<string, string>(Information,
        new EventId(id: 0, nameof(log_DocumentAccepted)),
        "Legal document with latest header '{Header}' is now accepted by user and saved to local preferences key '{PreferencesKey}', so user won't need to accept it again"
    );
    private void log_DocumentAccepted(LegalDocument legalDocument) =>
        LOG_DOC_ACCEPTED_ACTION(_logger!, legalDocument.TagHeader, legalDocument.PreferencesKey, null);


    private static readonly Action<MEL.ILogger, Exception?> LOG_ACCEPT_CLEARED_ACTION = LoggerMessage.Define(Information,
        new EventId(id: 0, nameof(log_AcceptCleared)),
        "Accepted tags for all legal documents have been cleared"
    );
    private void log_AcceptCleared() => LOG_ACCEPT_CLEARED_ACTION(_logger!, null);


    private static readonly Action<MEL.ILogger, string, string, Exception?> LOG_FETCH_LATEST_FAILED_ACTION = LoggerMessage.Define<string, string>(Warning,
        new EventId(id: 0, nameof(log_FetchLatestFailed)),
        "Unable to fetch latest version of legal document with {Uri}. Error received: {Error}"
    );
    private void log_FetchLatestFailed(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        LOG_FETCH_LATEST_FAILED_ACTION(_logger!, legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "", null);


    private static readonly Action<MEL.ILogger, string, string, Exception?> LOG_FETCH_LATEST_ERROR_CODE_ACTION = LoggerMessage.Define<string, string>(Warning,
        new EventId(id: 0, nameof(log_FetchLatestErrorCode)),
        "Unable to fetch latest version of legal document with {Uri}. Error received: {Error}"
    );
    private void log_FetchLatestErrorCode(LegalDocument legalDocument, UnityWebRequest? webRequest) =>
        LOG_FETCH_LATEST_ERROR_CODE_ACTION(_logger!, legalDocument.LatestVersionUri!.Uri, webRequest?.error ?? "", null);


    private static readonly Action<MEL.ILogger, string, string, Exception?> LOG_HEADER_PARSE_FAILED_1ST_TIME_ACTION = LoggerMessage.Define<string, string>(Warning,
        new EventId(id: 0, nameof(log_HeaderParseFailedFirstTime)),
        "Document tag from header '{Header}' was empty or could not be parsed. Using random GUID tag '{Tag}' instead."
    );
    private void log_HeaderParseFailedFirstTime(string header, string tag) =>
        LOG_HEADER_PARSE_FAILED_1ST_TIME_ACTION(_logger!, header, tag, null);


    private static readonly Action<MEL.ILogger, string, Exception?> LOG_HEADER_PARSE_FAILED_ACTION = LoggerMessage.Define<string>(Warning,
        new EventId(id: 0, nameof(log_HeaderParseFailed)),
        "Document tag from header '{Header}' was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again."
    );
    private void log_HeaderParseFailed(string header) =>
        LOG_HEADER_PARSE_FAILED_ACTION(_logger!, header, null);

    #endregion
}
