namespace UnityEngine {
    [CreateAssetMenu(fileName = "uri", menuName = nameof(UnityUtil) + "/" + nameof(OpenableUri))]
    public class OpenableUri : ScriptableObject {
        [Tooltip("The URI to be opened, using any of the protocols supported by Unity's Application.OpenURL API.")]
        [TextArea]
        public string Uri = "http://www.example.com";
        public void Open() => Device.Application.OpenURL(Uri);
    }
}
