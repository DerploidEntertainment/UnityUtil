using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;
using UE = UnityEngine;

namespace UnityUtil.Editor;

/// <summary>
/// Summarizes which objects with an attached <see cref="RemoveFromBuild"/> component were removed and which were preserved.
/// </summary>
public class RemovedObjectsReport
{
    /// <summary>
    /// Each key is the hierarchy name of a <see cref="GameObject"/> with an attached <see cref="RemoveFromBuild"/> component
    /// (see <see cref="UnityObjectExtensions.GetHierarchyName(GameObject, int, string)"/>).
    /// Each value is a string describing whether the object and/or its children were removed or preserved.
    /// </summary>
    public IReadOnlyDictionary<string, string> TargetResults { get; init; } = new Dictionary<string, string>();
}

public class BuildGameObjectRemover(ILoggerFactory loggerFactory)
{
    private static readonly Lazy<MethodInfo> TRY_CONVERT_TO_RUNTIME_PLATFORM = new(() => {
        // Reference the internal UnityEditor.BuildTargetConverter.TryConvertToRuntimePlatform() method
        // See https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/BuildTargetConverter.cs
        const string TYPE_NAME = "UnityEditor.BuildTargetConverter";
        const string METHOD_NAME = "TryConvertToRuntimePlatform";
        Type typeBuildTarget = typeof(BuildTarget);
        Type typeBuildTargetConverter = Assembly.GetAssembly(typeBuildTarget).GetType(TYPE_NAME, throwOnError: true, ignoreCase: false);
        return typeBuildTargetConverter.GetMethod(METHOD_NAME, genericParameterCount: 0, types: [typeBuildTarget])
            ?? throw new InvalidOperationException($"Could not find method '{METHOD_NAME}' on type '{typeBuildTargetConverter.FullName}'.");
    });

    private readonly ILogger<BuildGameObjectRemover> _logger = loggerFactory.CreateLogger<BuildGameObjectRemover>();

    /// <summary>
    /// Remove <see cref="GameObject"/>s from the provided <paramref name="scene"/> for the provided <paramref name="buildReport"/>.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="buildReport"></param>
    /// <returns>
    /// A <see cref="RemovedObjectsReport"/> summarizing which objects were removed and which were preserved.
    /// </returns>
    /// <exception cref="InvalidOperationException">Could not convert <paramref name="buildReport"/>'s <see cref="BuildTarget"/> to a <see cref="RuntimePlatform"/>.</exception>
    public RemovedObjectsReport RemoveGameObjectsFromScene(Scene scene, BuildReport buildReport)
    {
        BuildContext buildContext = buildReport.summary.options.HasFlag(BuildOptions.Development)
            ? BuildContext.DevelopmentBuild
            : BuildContext.NonDevelopmentBuild;

        RuntimePlatform platform = (RuntimePlatform?)TRY_CONVERT_TO_RUNTIME_PLATFORM.Value.Invoke(obj: null, parameters: [buildReport.summary.platform])    // Static method invoke
            ?? throw new InvalidOperationException($"Could not convert {nameof(BuildTarget)} '{buildReport.summary.platform}' to a {nameof(RuntimePlatform)}");

        return RemoveGameObjectsFromScene(scene, platform, buildContext);
    }

    /// <summary>
    /// Remove <see cref="GameObject"/>s from the provided <paramref name="scene"/> on the provided <paramref name="platform"/>.
    /// </summary>
    /// <param name="scene"><inheritdoc cref="RemoveGameObjectsFromScene(Scene, BuildReport)" path="/param[@name='scene']"/></param>
    /// <param name="platform">Platform for which a player is being built, or that is currently running in Play Mode.</param>
    /// <param name="buildContext"></param>
    /// <returns><inheritdoc cref="RemoveGameObjectsFromScene(Scene, BuildReport)"/></returns>
    public RemovedObjectsReport RemoveGameObjectsFromScene(Scene scene, RuntimePlatform platform, BuildContext buildContext)
    {
        Dictionary<string, string> targetResults = [];
        RemoveFromBuild[] removeTargets = [..
            scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<RemoveFromBuild>(includeInactive: true))
        ];  // For some reason the below loop never iterates if we don't enumerate this query first

        // Remove GameObjects and/or their children as necessary
        foreach (RemoveFromBuild removeTarget in removeTargets) {
            Transform removeTrans = removeTarget.transform;
            string targetHierarchyName = $"'{removeTrans.GetHierarchyName(parentCount: int.MaxValue)}'";

            string result;
            if (removeTarget.PreservePlatforms.Contains(platform!)
                && (removeTarget.PreserveBuildContexts & buildContext) != 0
            ) {
                result = $"Preserved due to matching {nameof(BuildContext)} and {nameof(RuntimePlatform)}";
            }
            else if (removeTarget.DestroyBehavior == DestroyBehavior.ChildrenOnly) {
                for (int ch = 0; ch < removeTrans.childCount; ++ch)
                    UE.Object.DestroyImmediate(removeTrans.GetChild(ch).gameObject);
                result = $"Removed {nameof(DestroyBehavior.ChildrenOnly)}";
            }
            else {
                UE.Object.DestroyImmediate(removeTrans.gameObject);
                result = $"Removed {nameof(DestroyBehavior.SelfAndChildren)}";
            }

            targetResults.Add(targetHierarchyName, result);
        }

        var report = new RemovedObjectsReport { TargetResults = targetResults };
        log_ObjectsRemoved(scene, buildContext, platform, report);

        return report;
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, BuildContext, RuntimePlatform, IReadOnlyDictionary<string, string>, Exception?> LOG_OBJECTS_REMOVED_ACTION =
        LoggerMessage.Define<string, BuildContext, RuntimePlatform, IReadOnlyDictionary<string, string>>(Information,
            new EventId(id: 0, nameof(log_ObjectsRemoved)),
            $"Results of removing GameObjects with attached {nameof(RemoveFromBuild)} components from scene '{{Scene}}' " +
            $"({nameof(BuildContext)}: '{{BuildContext}}', {nameof(RuntimePlatform)}: '{{Platform}}'):" +
            "\n{RemovedObjectsReport}"
        );
    private void log_ObjectsRemoved(Scene scene, BuildContext buildContext, RuntimePlatform platform, RemovedObjectsReport removedObjectsReport) =>
        LOG_OBJECTS_REMOVED_ACTION(_logger, scene.name, buildContext, platform, removedObjectsReport.TargetResults, null);

    #endregion
}
