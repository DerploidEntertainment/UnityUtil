using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Logging;

namespace UnityEngine.UI {
    public class VersionText : Configurable {
        public AppVersion Version;
        [Tooltip("This string is used to populate " + nameof(Text) + ". '{0}' will be replaced with " + nameof(Application.version) + ", '{1}' will be replaced with " + nameof(AppVersion.Description) + ", and '{2}' will be replaced with " + nameof(AppVersion.BuildNumber) + ". See here for details: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting")]
        public string FormatString;
        public Text Text;

        [Conditional("UNITY_EDITOR")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Reset() => FormatString = "Version {0}, \"{1}\" (build {2})";
        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(Text, nameof(Text));

            Text.text = string.Format(FormatString, Application.version, Version.Description, Version.BuildNumber);
        }
    }
}
