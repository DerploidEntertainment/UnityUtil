using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEditor;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Editor;

public class DependencyInjectorMenu
{
    /// <summary>
    /// Must be static to be used in menu commands in Unity Editor.
    /// This is an Editor script, so who cares if we hard-code the <see cref="ILoggerFactory"/>...
    /// </summary>
    private static readonly ILogger<DependencyInjectorMenu>? LOGGER = new UnityDebugLoggerFactory().CreateLogger<DependencyInjectorMenu>();

    public const string ItemName = $"{nameof(UnityUtil)}/Record dependency resolutions";

    [MenuItem(ItemName)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity MenuItem")]
    private static void toggleRecording()
    {
        DependencyResolutionCounts counts = DependencyInjector.Instance.GetServiceResolutionCounts();
        DependencyInjector.Instance.RecordingResolutions = !DependencyInjector.Instance.RecordingResolutions;
        if (DependencyInjector.Instance.RecordingResolutions)
            return;

        log_PrintRecording(counts.Uncached, counts.Cached);
    }

    [MenuItem(ItemName, isValidateFunction: true)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity MenuItem")]
    private static bool canToggleRecording()
    {
        Menu.SetChecked(ItemName, DependencyInjector.Instance.RecordingResolutions);
        return true;
    }

    #region LoggerMessages

    private static readonly Action<ILogger, IEnumerable<KeyValuePair<Type, int>>, IEnumerable<KeyValuePair<Type, int>>, Exception?> LOG_PRINT_RECORDING_ACTION =
        LoggerMessage.Define<IEnumerable<KeyValuePair<Type, int>>, IEnumerable<KeyValuePair<Type, int>>>(
            Information,
            new EventId(id: 0, nameof(log_PrintRecording)),
            $$"""
            Uncached dependency resolution counts:
            (If any of these counts are greater than 1, consider caching resolutions for that Type on the {{nameof(DependencyInjector)}} to improve performance)
            {CountsUncached}

            Cached dependency resolution counts:
            (If any of these counts equal 1, consider NOT caching resolutions for that Type on the {{nameof(DependencyInjector)}}, to speed up its resolutions and save memory)
            {CountsCached}
            """
        );
    private static void log_PrintRecording(IReadOnlyDictionary<Type, int> countsUncached, IReadOnlyDictionary<Type, int> countsCached) =>
        LOG_PRINT_RECORDING_ACTION(LOGGER!, countsUncached.OrderByDescending(x => x.Value), countsCached.OrderByDescending(x => x.Value), null);

    #endregion
}
