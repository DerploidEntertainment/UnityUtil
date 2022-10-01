using System.Globalization;
using UnityEngine;

namespace UnityUtil.Logging;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Logging)}/{nameof(FrameCountLogEnricher)}", fileName = "frame-count-log-enricher")]
public class FrameCountLogEnricher : LogEnricher
{
    [Tooltip(
        "'{0}' will be replaced by the current frame count. " +
        "Read more about .NET composite formatting here: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting"
    )]
    public string FormatString = "Frame {0}";

    public override string GetEnrichedLog(object source) => string.Format(CultureInfo.InvariantCulture, FormatString, Time.frameCount);
}
