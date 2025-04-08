using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Unity.Extensions.Logging;
using UnityEditor;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityUtil.Editor;

public class DependencyInjectorMenu
{
    private static ILogger<DependencyInjectorMenu>? s_logger;  // Must be static to be used as menu commands in Unity Editor

    public const string ItemName = $"{nameof(UnityUtil)}/Record dependency resolutions";

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Unity message")]
    private void Awake() => s_logger = new UnityDebugLoggerFactory().CreateLogger<DependencyInjectorMenu>();   // This is an Editor script...who cares if we hard-code the LoggerFactory

    [MenuItem(ItemName)]
    private static void toggleRecording()
    {
        DependencyResolutionCounts counts = DependencyInjector.Instance.ServiceResolutionCounts;
        DependencyInjector.Instance.RecordingResolutions = !DependencyInjector.Instance.RecordingResolutions;
        if (DependencyInjector.Instance.RecordingResolutions)
            return;

        s_logger!.DependencyInjectorPrintRecording(counts.Uncached, counts.Cached);
    }

    [MenuItem(ItemName, isValidateFunction: true)]
    private static bool canToggleRecording()
    {
        Menu.SetChecked(ItemName, DependencyInjector.Instance.RecordingResolutions);
        return true;
    }

}
