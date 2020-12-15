using UnityEngine.Logging;

namespace UnityEngine.UI
{
    public class VersionText : Configurable {

        private IAppVersion _appVersion;

        [Tooltip("This string is used to populate " + nameof(Text) + ". '{0}' will be replaced with the version set in Project Settings > Player, '{1}' will be replaced with " + nameof(AppVersion.Description) + ", and '{2}' will be replaced with " + nameof(AppVersion.BuildNumber) + ". See here for details: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting")]
        public string FormatString;
        public Text Text;

        public void Inject(IAppVersion appVersion) => _appVersion = appVersion;

        protected override void Reset()
        {
            base.Reset();

            FormatString = "Version {0}, \"{1}\" (build {2})";
        }

        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(Text, nameof(Text));

            Text.text = string.Format(FormatString, _appVersion.Version, _appVersion.Description, _appVersion.BuildNumber);
        }
    }
}
