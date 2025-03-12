using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;
using UnityUtil.Storage;

namespace UnityUtil.Legal;
internal class LegalDocumentState(string currentTag)
{
    public string CurrentTag = currentTag;
    public string? AcceptedTag;
}

public class LegalAcceptManager : MonoBehaviour, ILegalAcceptManager
{

    private LegalLogger<LegalAcceptManager>? _logger;
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
        _logger = new(loggerFactory, context: this);
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
            _logger!.LegalAcceptRequired(acceptOutdated);
            return acceptOutdated ? LegalAcceptance.Stale : LegalAcceptance.Unprovided;
        }
        else {
            _logger!.LegalAcceptAlreadyAcceptedAll();
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
            _logger!.LegalDocumentFetchLatestFailed(doc, req);
            req?.Dispose();
            tcs.SetException(ex);
        }

        return tcs.Task;


        void onRequestCompleted()
        {
            // Parse the tag from the response
            string? currentTag = null;
            if (req.result != UnityWebRequest.Result.Success)
                _logger!.LegalDocumentFetchLatestErrorCode(doc, req);
            else
                currentTag = req.GetResponseHeader(doc.TagHeader);

            req!.Dispose();

            // If unable to parse tag due to network or server errors, then
            // use a random GUID as the tag (shouldn't collide with an existing accepted tag) or the last accepted tag if one exists
            if (string.IsNullOrEmpty(currentTag)) {
                if (string.IsNullOrEmpty(acceptedTag)) {
                    currentTag = Guid.NewGuid().ToString();
                    _logger!.LegalDocumentHeaderParseFailedFirstTime(doc.TagHeader, currentTag);
                }
                else {
                    currentTag = acceptedTag;
                    _logger!.LegalDocumentHeaderParseFailed(doc.TagHeader);
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
            _logger!.LegalDocumentAccepted(Documents[v]);
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

        _logger ??= new(new UnityDebugLoggerFactory(), context: this);    // Use debug logger in case this is being run from the Inspector outside Play mode
        _logger.LegalAcceptCleared();
    }

}
