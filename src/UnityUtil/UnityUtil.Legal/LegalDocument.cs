using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Legal;

/// <summary>
/// Represents a versioned legal document that users must accept before using the application.
/// </summary>
[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Legal)}/{nameof(LegalDocument)}", fileName = "policy.asset")]
public class LegalDocument : ScriptableObject
{
    [Tooltip(
        "The URI that points at the latest version of this legal document. " +
        "For obvious reasons, the server response for this resource should not include cache headers (other than cache validation)."
    )]
    [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
    public OpenableUri? LatestVersionUri;

    [Tooltip(
        $"The server response for the resource located at {nameof(LatestVersionUri)} must include this header, " +
        "containing a unique tag for the document version that can be stored in preferences."
    )]
    public string TagHeader = "ETag";

    [Tooltip(
        $"After a user accepts the latest version of this legal document, that version's tag (from the {nameof(TagHeader)}) " +
        "will be stored in preferences, so that the user doesn't have to accept again until the document is updated with a new tag."
    )]
    public string PreferencesKey = "ACCEPTED_POLICY_ETAG";
}
