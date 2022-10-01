using System.Globalization;
using UnityEngine;

namespace UnityUtil.Logging;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Logging)}/{nameof(TypeNameLogEnricher)}", fileName = "type-name-log-enricher")]
public class TypeNameLogEnricher : LogEnricher
{
    [Tooltip(
        "'{0}' will be replaced by the name of the type. " +
        "Read more about .NET composite formatting here: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"
    )]
    public string FormatString = "Type {0}";

    public override string GetEnrichedLog(object source) =>
        source == null ? "" : string.Format(CultureInfo.InvariantCulture, FormatString, source.GetType().Name);
}
