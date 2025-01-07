namespace UnityUtil.BuildAutomation;

/// <summary>
/// Represents a Unity Build Automation (UBA) build manifest, for runtime parsing of the JSON manifest provided by UBA.
/// See the <a href="https://docs.unity.com/ugs/en-us/manual/devops/manual/build-automation/build-automation-integrations/use-build-automations-build-manifest-during-runtime">UBA build manifest docs</a>
/// for more info.
/// </summary>
public sealed class BuildManifest
{
    /// <summary>
    /// The commit or changelist that was built.
    /// </summary>
    public string? ScmCommitId { get; set; }

    /// <summary>
    /// The name of the branch that was built.
    /// </summary>
    public string? ScmBranch { get; set; }

    /// <summary>
    /// The Build Automation build number corresponding to this build.
    /// </summary>
    public string? BuildNumber { get; set; }

    /// <summary>
    /// The UTC timestamp when the build process started.
    /// </summary>
    public string? BuildStartTime { get; set; }

    /// <summary>
    /// The Unity project identifier.
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// The bundleIdentifier configured in Build Automation (iOS and Android only).
    /// </summary>
    public string? BundleId { get; set; }

    /// <summary>
    /// The version of Unity that Build Automation used to create the build.
    /// </summary>
    public string? UnityVersion { get; set; }

    /// <summary>
    /// The version of XCode used to build the Project (iOS only).
    /// </summary>
    public string? XcodeVersion { get; set; }

    /// <summary>
    /// The name of the build target that was built.
    /// </summary>
    public string? CloudBuildTargetName { get; set; }
}
