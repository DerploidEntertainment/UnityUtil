using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    [CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(AppVersion)}", fileName = "version")]
    public class AppVersion : ScriptableObject, IAppVersion
    {

        [ReadOnly, ShowInInspector, LabelText(nameof(Version))]
        [PropertyTooltip("This is the version set under Project Settings > Player")]
        public string Version => Device.Application.version;

        [field: SerializeField, ShowInInspector, LabelText(nameof(Description))]
        [field: Tooltip("This can be any string to describe the version, but will usually be a short phrase like 'The Monster Update'. The numerical, semantic version of the app should be set in Project Settings > Player.")]
        public string Description { get; set; }
        [field: SerializeField, ShowInInspector, LabelText(nameof(BuildNumber))]
        [field: Tooltip("This number represents the build number from the continuous deployment system, such as Unity Cloud Build.")]
        public int BuildNumber { get; set; }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void Reset() {
            Description = "Initial Release";
            BuildNumber = 1;
        }
    }
}
