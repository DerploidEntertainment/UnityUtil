﻿namespace UnityEngine {
    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(LegalDocument), fileName = "policy.asset")]
    public class LegalDocument : ScriptableObject {
        [Tooltip("The URI that points at the latest version of this legal document. For obvious reasons, the server response for this resource should not include cache headers (other than cache validation).")]
        public OpenableUri LatestVersionUri;
        [Tooltip("The server response for the resource located at " + nameof(LatestVersionUri) + " must include this header, containing a unique tag for the document version that can be stored in PlayerPrefs.")]
        public string TagHeader = "ETag";
        [Tooltip("After a user accepts the latest version of this legal document, that version's tag (from the "+nameof(TagHeader)+") will be stored in PlayerPrefs, so that the user doesn't have to accept again until the document is updated with a new tag.")]
        public string AcceptPlayerPrefKey = "ACCEPTED_POLICY_ETAG";
    }

}
