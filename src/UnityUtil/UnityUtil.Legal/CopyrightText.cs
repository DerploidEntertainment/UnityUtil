using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UD = UnityEngine.Device;

namespace UnityUtil.Legal;

public class CopyrightText : MonoBehaviour
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

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public TMP_Text? Text;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake() =>
        Text!.text = string.Format(CultureInfo.CurrentCulture, FormatString, DateTime.Now, UD.Application.companyName);
}
