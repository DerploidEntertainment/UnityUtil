using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine.Logging;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UnityEngine {

    public class LegalAcceptManager : Configurable {

        private ILogger _logger;

        private string[] _latestVersionTags;
        private bool _acceptRequired = false;
        private bool _acceptUpdate = false;
        private int _numTagsFetched = 0;

        public LegalDocument[] Documents;

        [Tooltip("This UI object will be activated if no version tags of the provided legal documents have been accepted yet.")]
        public GameObject AcceptInitialUi;
        [Tooltip("This UI object will be activated if the version tags of the provided legal documents on the web don't match those that have already been accepted.")]
        public GameObject AcceptUpdateUi;
        [Tooltip("This UI object will only be activated once all documents have been checked for updates, so that players don't 'accept' before there is actually anything ready to accept.")]
        public GameObject AcceptUi;

        [Tooltip("If the player has already accepted the latest versions of all legal documents, then this event will be raised instead.")]
        public UnityEvent AlreadyAccepted = new UnityEvent();

        public void Inject(ILoggerProvider loggerProvider) {
            _logger = loggerProvider.GetLogger(this);
        }

        protected override void OnAwake() {
            base.OnAwake();

            this.AssertAssociation(AcceptInitialUi, nameof(AcceptInitialUi));
            this.AssertAssociation(AcceptUpdateUi, nameof(AcceptUpdateUi));

            _latestVersionTags = new string[Documents.Length];

            AcceptInitialUi.SetActive(false);
            AcceptUpdateUi.SetActive(false);
            AcceptUi.SetActive(false);

            for (int d = 0; d < Documents.Length; ++d)
                checkForUpdate(Documents[d], d);


            void checkForUpdate(LegalDocument doc, int index) {

                // Get the last accepted tag from PlayerPrefs (will be empty if none stored yet)
                string acceptedTag = PlayerPrefs.GetString(doc.AcceptPlayerPrefKey);
                bool firstTime = string.IsNullOrEmpty(acceptedTag);

                // Get the latest tag from the web
                var req = UnityWebRequest.Head(doc.LatestVersionUri.Uri);
                UnityWebRequestAsyncOperation reqOp = req.SendWebRequest();
                reqOp.completed += op => {
                    // Parse the tag from the response
                    string webTag = null;
                    if (req.isNetworkError || req.isHttpError)
                        _logger.LogWarning($"Unable to fetch latest version of legal document with URI '{doc.LatestVersionUri.Uri}'. Error received: {req.error}", context: this);
                    else
                        webTag = req.GetResponseHeader(doc.TagHeader);

                    // If unable to parse tag due to network or server errors, then
                    // Use a random GUID as the tag (shouldn't collide with an existing accepted tag), unless user has already accepted this document once before
                    if (string.IsNullOrEmpty(webTag)) {
                        if (firstTime) {
                            _logger.LogWarning($"Document tag from '{doc.TagHeader}' header was empty or could not be parsed. Using random GUID '{webTag}' instead.", context: this);
                            webTag = Guid.NewGuid().ToString();
                        }
                        else {
                            _logger.LogWarning($"Document tag from '{doc.TagHeader}' header was empty or could not be parsed. User has already accepted a previous version, so acceptance won't be required again.", context: this);
                            webTag = acceptedTag;
                        }
                    }

                    ++_numTagsFetched;
                    _latestVersionTags[index] = webTag;

                    // If the tag from the web does not match the version in PlayerPrefs, then
                    // Show the "accept" text or the "accept an update" text, depending on whether PlayerPref tag existed
                    _acceptRequired = _acceptRequired || (webTag != acceptedTag);
                    _acceptUpdate = _acceptUpdate || (_acceptRequired && !firstTime);
                    if (_numTagsFetched == Documents.Length) {
                        if (_acceptRequired) {
                            AcceptInitialUi.SetActive(!_acceptUpdate);
                            AcceptUpdateUi.SetActive(_acceptUpdate);
                            AcceptUi.SetActive(true);
                            _logger.Log($"User must accept latest versions of all legal documents {(_acceptUpdate ? "because the versions that they last accepted are out of date" : "for the first time", context: this)}.");
                        }
                        else {
                            _logger.Log($"User already accepted latest versions of all legal documents.", context: this);
                            AlreadyAccepted.Invoke();
                        }
                    }
                };
            }
        }

        public void Accept() {
            for (int v = 0; v < _latestVersionTags.Length; ++v)
                PlayerPrefs.SetString(Documents[v].AcceptPlayerPrefKey, _latestVersionTags[v].ToString());
            _logger.Log($"User has accepted latest versions all legal documents.", context: this);
        }

        [Button, Conditional("DEBUG")]
        public void ClearAcceptance() {
            for (int d = 0; d < Documents.Length; ++d)
                PlayerPrefs.DeleteKey(Documents[d].AcceptPlayerPrefKey);

            ILogger logger = (_logger ?? Debug.unityLogger);    // Use debug logger in case this is being run from the Inspector outside Play mode
            logger.Log($"Accepted tags for all legal documents have been cleared.", context: this);
        }

    }

}
