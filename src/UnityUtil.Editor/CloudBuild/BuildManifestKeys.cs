using System.Collections.Generic;

namespace UnityEngine.CloudBuild {
    /// <summary>
    /// Contains strings for retrieving key-values from the Unity Cloud Build manifest.
    /// See the Unity Manual for more info: https://docs.unity3d.com/Manual/UnityCloudBuildManifest.html
    /// </summary>
    public static class BuildManifestKeys {
        /// <summary>The commit or changelist that was built.</summary>
        public const string ScmCommitId = "scmCommitId";

        /// <summary>The name of the branch that was built.</summary>
        public const string ScmBranch = "scmBranch";

        /// <summary>The Unity Cloud Build "build number" corresponding to this build.</summary>
        public const string BuildNumber = "buildNumber";

        /// <summary>The UTC timestamp when the build process was started.</summary>
        public const string BuildStartTime = "buildStartTime";

        /// <summary>The Unity project identifier.</summary>
        public const string ProjectId = "projectId";

        /// <summary>The bundleIdentifier configured in Unity Cloud Build (iOS and Android only".</summary>
        public const string BundleId = "bundleId";

        /// <summary>The version of Unity that Unity Cloud Build used to create the build.</summary>
        public const string UnityVersion = "unityVersion";

        /// <summary>The version of XCode used to build the project (iOS only".</summary>
        public const string XcodeVersion = "xcodeVersion";

        /// <summary>The name of the build target that was built.</summary>
        public const string CloudBuildTargetName = "cloudBuildTargetName";

        public static IReadOnlyCollection<string> AllKeysForPlatform(RuntimePlatform platform) {
            var keys = new List<string> {
                ScmCommitId,
                ScmBranch,
                BuildNumber,
                BuildStartTime,
                ProjectId,
                UnityVersion,
                CloudBuildTargetName,
            };

            if (platform == RuntimePlatform.IPhonePlayer)
                keys.Add(XcodeVersion);

            if (platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android)
                keys.Add(BundleId);

            return keys;
        }

    }
}
