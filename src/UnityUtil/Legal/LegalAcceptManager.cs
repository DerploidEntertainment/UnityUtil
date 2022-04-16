using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.Networking;
using UnityEngine.Storage;
using UnityEngine.UI;

namespace UnityEngine.Legal;

public class LegalAcceptManager : Configurable
{
    private ILogger? _logger;
    private ILocalCache? _localCache;

    private string[] _latestVersionTags = Array.Empty<string>();
    private bool _acceptRequired;
    private bool _acceptUpdate;
    private int _numTagsFetched;

    public LegalDocument[] Documents = Array.Empty<LegalDocument>();

    [Tooltip("This UI object will be activated if no version tags of the provided legal documents have been accepted yet.")]
    [Required] public GameObject? AcceptInitialUi;

    [Tooltip("This UI object will be activated if the version tags of the provided legal documents on the web don't match those that have already been accepted.")]
    [Required] public GameObject? AcceptUpdateUi;

    [Tooltip("This UI object will only be activated once all documents have been checked for updates, so that players don't 'accept' before there is actually anything ready to accept.")]
    [Required] public GameObject? AcceptUi;

    [Tooltip("If the player has already accepted the latest versions of all legal documents, then this event will be raised instead.")]
    public UnityEvent AlreadyAccepted = new();

    public void Inject(ILoggerProvider loggerProvider, ILocalCache localCache)
    {
        _logger = loggerProvider.GetLogger(this);
        _localCache = localCache;
    }

    protected override void Awake()
    {
        base.Awake();

        _latestVersionTags = new string[Documents.Length];

        AcceptInitialUi!.SetActive(false);
        AcceptUpdateUi!.SetActive(false);
        AcceptUi!.SetActive(false);

        for (int d = 0; d < Documents.Length; ++d)
            CheckForUpdate(Documents[d], d);
    }
    internal void CheckForUpdate(LegalDocument doc, int index, DownloadHandler? downloadHandler = null, UploadHandler? uploadHandler = null)
    {
        if (downloadHandler is null ^ uploadHandler is null)
            throw new InvalidOperationException();

        // Get the last accepted tag from the cache (will be empty if none stored yet)
        string acceptedTag = _localCache!.GetString(doc.CacheKey);
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

            ++_numTagsFetched;
            _latestVersionTags[index] = webTag;

            // If the tag from the web does not match the version in cache, then
            // Show the "accept" text or the "accept an update" text, depending on whether cached tag existed
            _acceptRequired = _acceptRequired || (webTag != acceptedTag);
            _acceptUpdate = _acceptUpdate || (_acceptRequired && !firstTime);
            if (_numTagsFetched == Documents.Length) {
                if (_acceptRequired) {
                    AcceptInitialUi!.SetActive(!_acceptUpdate);
                    AcceptUpdateUi!.SetActive(_acceptUpdate);
                    AcceptUi!.SetActive(true);
                    _logger!.Log($"User must accept latest versions of all legal documents {(_acceptUpdate ? "because the versions that they last accepted are out of date" : "for the first time")}.", context: this);
                }
                else {
                    _logger!.Log($"User already accepted latest versions of all legal documents.", context: this);
                    AlreadyAccepted.Invoke();
                }
            }
        }
    }
    public bool HasAccepted { get; private set; }

    public void Accept()
    {
        for (int v = 0; v < _latestVersionTags.Length; ++v)
            _localCache!.SetString(Documents[v].CacheKey, _latestVersionTags[v].ToString());

        HasAccepted = true;

        _logger!.Log($"User has accepted latest versions all legal documents.", context: this);
    }

    [Button, Conditional("DEBUG")]
    public void ClearAcceptance()
    {
        if (_localCache == null) {
            for (int d = 0; d < Documents.Length; ++d)
                PlayerPrefs.DeleteKey(Documents[d].CacheKey);
        }

        // Use PlayerPrefs in case this is being run from the Inspector outside Play mode
        else {
            for (int d = 0; d < Documents.Length; ++d)
                _localCache.DeleteKey(Documents[d].CacheKey);
        }

        ILogger logger = (_logger ?? Debug.unityLogger);    // Use debug logger in case this is being run from the Inspector outside Play mode
        logger.Log($"Accepted tags for all legal documents have been cleared.", context: this);
    }

}
