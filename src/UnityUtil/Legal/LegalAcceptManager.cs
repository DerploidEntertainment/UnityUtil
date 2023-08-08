using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;
using UnityUtil.Storage;

namespace UnityUtil.Legal;

public class LegalAcceptManager : MonoBehaviour, ILegalAcceptManager
{
    private LegalLogger<LegalAcceptManager>? _logger;
    private ILocalPreferences? _localPreferences;

    private string[] _latestVersionTags = Array.Empty<string>();
    private bool _acceptRequired;
    private bool _acceptOutdated;
    private int _numTagsFetched;

    public LegalDocument[] Documents = Array.Empty<LegalDocument>();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory, ILocalPreferences localPreferences)
    {
        _logger = new(loggerFactory, context: this);
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
            _logger!.LegalDocumentFetchLatesetFailed(doc, req);
            req?.Dispose();
        }


        void onRequestCompleted(UnityWebRequest request)
        {
            // Parse the tag from the response
            string? webTag = null;
            if (request.result != UnityWebRequest.Result.Success)
                _logger!.LegalDocumentFetchLatesetErrorCode(doc, request);
            else
                webTag = request.GetResponseHeader(doc.TagHeader);

            request.Dispose();

            // If unable to parse tag due to network or server errors, then
            // Use a random GUID as the tag (shouldn't collide with an existing accepted tag), unless user has already accepted this document once before
            if (string.IsNullOrEmpty(webTag)) {
                if (firstTime) {
                    webTag = Guid.NewGuid().ToString();
                    _logger!.LegalDocumentHeaderParseFailedFirstTime(doc.TagHeader, webTag);
                }
                else {
                    webTag = acceptedTag;
                    _logger!.LegalDocumentHeaderParseFailed(doc.TagHeader);
                }
            }

            _latestVersionTags[documentIndex] = webTag;

            // If the tag from the web does not match the version in preferences, then
            // Show the "accept" text or the "accept an update" text, depending on whether preferences tag existed
            _acceptRequired |= (webTag != acceptedTag);
            _acceptOutdated |= (_acceptRequired && !firstTime);
            if (++_numTagsFetched == Documents.Length) {
                if (_acceptRequired) {
                    _logger!.LegalAcceptRequired(_acceptOutdated);
                    callback(_acceptOutdated ? LegalAcceptance.Stale : LegalAcceptance.Unprovided);
                }
                else {
                    _logger!.LegalAcceptAlreadyAcceptedAll();
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
