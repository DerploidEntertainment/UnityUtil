using Microsoft.Extensions.Logging;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtil.Logging;
using Application = UnityEngine.Device.Application;

namespace UnityUtil.Editor;

public class BuildGameObjectRemover
{
    private readonly EditorRootLogger<BuildGameObjectRemover> _logger;

    public BuildGameObjectRemover(ILoggerFactory loggerFactory, ObjectNameLogEnrichSettings objectNameLogEnrichSettings) =>
        _logger = new(loggerFactory, objectNameLogEnrichSettings, context: this);

    public void RemoveGameObjectsFromScene(Scene scene)
    {
        RuntimePlatform platform = Application.platform;
        BuildContext buildContext =
            Application.isPlaying ? BuildContext.PlayMode
            : Debug.isDebugBuild ? BuildContext.DebugBuild
            : BuildContext.ReleaseBuild;

        int numRemoveTargets = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int r = 0; r < roots.Length; ++r) {
            GameObject root = roots[r];

            RemoveFromBuild[] removableTargets = root.GetComponentsInChildren<RemoveFromBuild>(includeInactive: true);
            numRemoveTargets += removableTargets.Length;

            // Determine which GameObjects should actually be removed
            RemoveFromBuild[] targetsToRemove = removableTargets
                .Where(x => !(x.PreservePlatforms.Contains(platform) && (x.PreserveBuildContexts & buildContext) != 0))
                .ToArray();

            // Remove them (and/or their children)!
            for (int t = 0; t < targetsToRemove.Length; ++t) {
                RemoveFromBuild removeTarget = targetsToRemove[t];
                Transform removeTrans = removeTarget.transform;
                if (removeTarget.DestroyBehavior == DestroyBehavior.ChildrenOnly) {
                    for (int ch = 0; ch < removeTrans.childCount; ++ch)
                        Object.DestroyImmediate(removeTrans.GetChild(ch));
                }
                else if (removeTarget.DestroyBehavior == DestroyBehavior.SelfAndChildren)
                    Object.DestroyImmediate(removeTrans.gameObject);
            }

            if (removableTargets.Length > 0)
                _logger.ContextualGameObjectsRemoved(targetsToRemove, removableTargets, root, scene);
        }

        if (numRemoveTargets == 0)
            _logger.ContextualGameObjectsNotRemoved(scene);
    }
}
