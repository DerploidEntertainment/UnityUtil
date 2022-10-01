using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Storage;
using UnityEngine.UI;
using UnityUtil.Configuration;
using UnityUtil.Logging;
using U = UnityEngine;

namespace UnityUtil.Legal;

public class LegalAcceptManager : Configurable, ILegalAcceptManager
{
    private ILogger? _logger;
    private ILocalPreferences? _localPreferences;

    private string[] _latestVersionTags = Array.Empty<string>();
    private bool _acceptRequired;
    private bool _acceptUpdate;
    private int _numTagsFetched;

    public LegalDocument[] Documents = Array.Empty<LegalDocument>();

    public void Inject(ILoggerProvider loggerProvider, ILocalPreferences localPreferences)
    {
        _logger = loggerProvider.GetLogger(this);
        _localPreferences = localPreferences;
    }

    public void CheckAcceptance(Action<LegalAcceptance> callback)
    {
        _latestVersionTags = new string[Documents.Length];
        for (int d = 0; d < Documents.Length; ++d)
            CheckForUpdate(d, callback);
    }

    internal void CheckForUpdate(int documentIndex, Action<LegalAcceptance> callback, DownloadHandler? downloadHandler = null, UploadHandler? uploadHandler = null)
    {
        if (downloadHandler is null ^ uploadHandler is null)
            throw new InvalidOperationException($"{nameof(downloadHandler)} and {nameof(uploadHandler)} must be both null or both non-null");

        // Get the last accepted tag from preferences (will be empty if none stored yet)
        LegalDocument doc = Documents[documentIndex];
        string acceptedTag = _localPreferences!.GetString(doc.PreferencesKey);
        bool firstTime = string.IsNullOrEmpty(acceptedTag);

        UnityWebRequest? req = null;
        try {
            // Get the latest tag from the web
#pragma warning disable CA2000 // Dispose objects before losing scope
            req = downloadHandler is null
                ? new UnityWebRequest(doc.LatestVersionUri!.Uri, UnityWebRequest.kHttpVerbHEAD)
                : new UnityWebRequest(doc.LatestVersionUri!.Uri, UnityWebRequest.kHttpVerbHEAD, downloadHandler, uploadHandler);
#pragma warning restore CA2000 // Dispose objects before losing scope
            UnityWebRequestAsyncOperation reqOp = req.SendWebRequest();
            reqOp.completed += op => onRequestCompleted(req);   // Request must be explicitly disposed in here
        }
        catch {
            _logger!.LogWarning($"Unable to fetch latest version of legal document with URI '{doc.LatestVersionUri!.Uri}'. Error received: {req?.error ?? ""}", context: this);
            req?.Dispose();
        }


        void onRequestCompleted(UnityWebRequest request)
        {
            // Parse the tag from the response
            string? webTag = null;
            if (request.result != UnityWebRequest.Result.Success)
                _logger!.LogWarning($"Unable to fetch latest version of legal document with URI '{doc.LatestVersionUri.Uri}'. Error received: {request.error}", context: this);
            else
                webTag = request.GetResponseHeader(doc.TagHeader);

            request.Dispose();

            // If unable to parse tag due to network or server errors, then
            // Use a random GUID as the tag (shouldn't collide with an existing accepted tag), unless user has already accepted this document once before
            if (string.IsNullOrEmpty(webTag)) {
                if (firstTime) {
                    _logger!.LogWarning($"Document tag from '{doc.TagHeader}' header was empty or could not be parsed. Using random GUID '{webTag}' instead.", context: this);
                    webTag = Guid.NewGuid().ToString();
                }
                else {
                    _logger!.LogWarning($"Document tag from '{doc.TagHeader}' header was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again.", context: this);
                    webTag = acceptedTag;
                }
            }

            _latestVersionTags[documentIndex] = webTag;

            // If the tag from the web does not match the version in preferences, then
            // Show the "accept" text or the "accept an update" text, depending on whether preferences tag existed
            _acceptRequired |= (webTag != acceptedTag);
            _acceptUpdate |= (_acceptRequired && !firstTime);
            if (++_numTagsFetched == Documents.Length) {
                if (_acceptRequired) {
                    _logger!.Log($"User must accept latest versions of all legal documents {(_acceptUpdate ? "because the versions that they last accepted are out of date" : "for the first time")}.", context: this);
                    callback(_acceptUpdate ? LegalAcceptance.Stale : LegalAcceptance.Unprovided);
                }
                else {
                    _logger!.Log($"User already accepted latest versions of all legal documents.", context: this);
                    callback(LegalAcceptance.Current);
                }
            }
        }
    }
    public bool HasAccepted { get; private set; }

    public void Accept()
    {
        for (int v = 0; v < _latestVersionTags.Length; ++v) {
            _localPreferences!.SetString(Documents[v].PreferencesKey, _latestVersionTags[v].ToString());
            _logger!.Log($"Legal document with latest '{Documents[v].TagHeader}' header is now accepted by user and saved to local preferences at '{Documents[v].PreferencesKey}', so user won't need to accept it again.", context: this);
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

        ILogger logger = (_logger ?? U.Debug.unityLogger);    // Use debug logger in case this is being run from the Inspector outside Play mode
        logger.Log($"Accepted tags for all legal documents have been cleared.", context: this);
    }

}
