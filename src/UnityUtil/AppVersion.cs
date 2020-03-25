using System.Diagnostics.CodeAnalysis;

namespace UnityEngine {
    [CreateAssetMenu(menuName = "UnityUtil" + "/" + nameof(AppVersion), fileName = "version")]
    public class AppVersion : ScriptableObject {
        [Tooltip("This can be any string to describe the version, but will usually be a short phrase like 'The Monster Update'. The numerical, semantic version of the app should be set in Project Settings > Player.")]
        public string Description;
        [Tooltip("This number represents the build number from the continuous deployment system, such as Unity Cloud Build.")]
        public int BuildNumber;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() {
            Description = "Initial Release";
            BuildNumber = 1;
        }
    }
}
