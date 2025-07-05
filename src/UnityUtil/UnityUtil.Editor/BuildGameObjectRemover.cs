using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Microsoft.Extensions.Logging.LogLevel;
using Application = UnityEngine.Device.Application;
using MEL = Microsoft.Extensions.Logging;
using UE = UnityEngine;

namespace UnityUtil.Editor;

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

    public void RemoveGameObjectsFromScene(Scene scene, BuildTarget buildTarget)
    {
        RuntimePlatform platform = convertBuildTargetToRuntimePlatform(buildTarget);
        BuildContext buildContext =
            (Application.isEditor && Application.isPlaying) ? BuildContext.PlayMode
            : EditorUserBuildSettings.development ? BuildContext.DevelopmentBuild
            : BuildContext.NonDevelopmentBuild;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int r = 0; r < roots.Length; ++r) {
            GameObject root = roots[r];

            RemoveFromBuild[] removableTargets = root.GetComponentsInChildren<RemoveFromBuild>(includeInactive: true);

            // Determine which GameObjects should actually be removed
            RemoveFromBuild[] targetsToRemove = [.. removableTargets
                .Where(x => !(x.PreservePlatforms.Contains(platform!) && (x.PreserveBuildContexts & buildContext) != 0))
            ];

            // Remove them (and/or their children)!
            for (int t = 0; t < targetsToRemove.Length; ++t) {
                RemoveFromBuild removeTarget = targetsToRemove[t];
                Transform removeTrans = removeTarget.transform;
                if (removeTarget.DestroyBehavior == DestroyBehavior.ChildrenOnly) {
                    for (int ch = 0; ch < removeTrans.childCount; ++ch)
                        UE.Object.DestroyImmediate(removeTrans.GetChild(ch));
                }
                else if (removeTarget.DestroyBehavior == DestroyBehavior.SelfAndChildren)
                    UE.Object.DestroyImmediate(removeTrans.gameObject);
            }

            if (removableTargets.Length > 0)
                log_ObjectsRemoved(targetsToRemove, removableTargets, root, scene, buildTarget);
        }
    }

    private static RuntimePlatform convertBuildTargetToRuntimePlatform(BuildTarget buildTarget) =>
        (RuntimePlatform?)TRY_CONVERT_TO_RUNTIME_PLATFORM.Value.Invoke(obj: null, parameters: [buildTarget])    // Static method invoke
        ?? throw new InvalidOperationException($"Could not convert {nameof(BuildTarget)} '{buildTarget}' to a {nameof(RuntimePlatform)}");

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, int, int, string, string, BuildTarget, Exception?> LOG_OBJECTS_REMOVED_ACTION =
        LoggerMessage.Define<int, int, string, string, BuildTarget>(Information,
            new EventId(id: 0, nameof(log_ObjectsRemoved)),
            $"{{CountTargetsRemoved}} out of {{CountTargetsRemovable}} GameObjects with attached {nameof(RemoveFromBuild)} components under root object '{{Root}}' in scene '{{Scene}}' actually fit the criteria for contextual removal and were removed. Build target: '{{BuildTarget}}'"
        );
    private void log_ObjectsRemoved(RemoveFromBuild[] targetsRemoved, RemoveFromBuild[] targetsRemovable, GameObject parent, Scene scene, BuildTarget buildTarget) =>
        LOG_OBJECTS_REMOVED_ACTION(_logger, targetsRemoved.Length, targetsRemovable.Length, parent.name, scene.name, (BuildTarget)(RuntimePlatform)buildTarget, null);

    #endregion
}
