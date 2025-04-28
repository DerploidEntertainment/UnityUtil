using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Microsoft.Extensions.Logging.LogLevel;
using Application = UnityEngine.Device.Application;
using MEL = Microsoft.Extensions.Logging;
using UE = UnityEngine;

namespace UnityUtil.Editor;

public class BuildGameObjectRemover(ILoggerFactory loggerFactory)
{
    private readonly ILogger<BuildGameObjectRemover> _logger = loggerFactory.CreateLogger<BuildGameObjectRemover>();

    public void RemoveGameObjectsFromScene(Scene scene)
    {
        RuntimePlatform platform = Application.platform;
        BuildContext buildContext =
            Application.isPlaying ? BuildContext.PlayMode
            : UE.Debug.isDebugBuild ? BuildContext.DebugBuild
            : BuildContext.ReleaseBuild;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int r = 0; r < roots.Length; ++r) {
            GameObject root = roots[r];

            RemoveFromBuild[] removableTargets = root.GetComponentsInChildren<RemoveFromBuild>(includeInactive: true);

            // Determine which GameObjects should actually be removed
            RemoveFromBuild[] targetsToRemove = [.. removableTargets
                .Where(x => !(x.PreservePlatforms.Contains(platform) && (x.PreserveBuildContexts & buildContext) != 0))
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
                log_ObjectsRemoved(targetsToRemove, removableTargets, root, scene);
        }
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, int, int, string, string, Exception?> LOG_OBJECTS_REMOVED_ACTION =
        LoggerMessage.Define<int, int, string, string>(Information,
            new EventId(id: 0, nameof(log_ObjectsRemoved)),
            "Removed {CountTargetsRemoved} GameObjects out of {CountTargetsRemovable} marked for contextual removal under parent '{Parent}' in scene '{Scene}'"
        );
    private void log_ObjectsRemoved(RemoveFromBuild[] targetsRemoved, RemoveFromBuild[] targetsRemovable, GameObject parent, Scene scene) =>
        LOG_OBJECTS_REMOVED_ACTION(_logger, targetsRemoved.Length, targetsRemovable.Length, parent.name, scene.name, null);

    #endregion
}
