﻿using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityUtil.DependencyInjection;

namespace UnityUtil.UI;

public class VersionText : MonoBehaviour
{
    private IAppVersion? _appVersion;

    [Tooltip(
        $"This string is used to populate {nameof(Text)}. " +
        $"'{{0}}' will be replaced with {nameof(Application)}.{nameof(Application.version)} (from Project Settings > Player > Other settings > Identification > Version), " +
        $"'{{1}}' will be replaced with {nameof(AppVersion.Description)}, and " +
        $"'{{2}}' will be replaced with {nameof(AppVersion.BuildNumber)} (in user's culture). " +
        "See here for details on string composite formatting: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"
    )]
    public string FormatString = "Version {0}, \"{1}\" (build {2})";

    [Tooltip(
        $"This component's text will be populated with a string generated by formatting {nameof(FormatString)} with specific values. " +
        $"See {nameof(FormatString)}'s tooltip for a description of those values."
    )]
    [Required]
    public TMP_Text? Text;

    public void Inject(IAppVersion appVersion) => _appVersion = appVersion;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake()
    {
        DependencyInjector.Instance.ResolveDependenciesOf(this);

        Text!.text = string.Format(CultureInfo.CurrentCulture, FormatString, _appVersion!.Version, _appVersion.Description, _appVersion.BuildNumber);
    }
}
