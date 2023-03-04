using Sirenix.OdinInspector;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityUtil.Configuration;
using UD = UnityEngine.Device;

namespace UnityUtil.Legal;

public class CopyrightText : Configurable
{
    [Tooltip(
        $"This string is used to populate {nameof(Text)}. " +
        $"'{{0}}' will be replaced with the current date (in user's culture) and " +
        $"'{{1}}' will be replaced with {nameof(UD.Application)}.{nameof(UD.Application.companyName)}, " +
        $"using .NET composite formatting. For example, '{{0:yyyy}}' would be replaced with just the current 4-digit year. " +
        $"See here for details: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"
    )]
    [MultiLineProperty]
    public string FormatString = "© {0}, {1}";

    [Required]
    public TMP_Text? Text;

    protected override void Awake()
    {
        base.Awake();

        Text!.text = string.Format(CultureInfo.CurrentCulture, FormatString, DateTime.Now, UD.Application.companyName);
    }
}
