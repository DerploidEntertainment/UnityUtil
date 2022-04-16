using System.Linq;
using UnityEngine;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Device.Application;

namespace UnityUtil.Editor;

public class BuildGameObjectRemover
{
    private readonly ILogger _logger;

    public BuildGameObjectRemover(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

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
                _logger.Log($"Removed {targetsToRemove.Length} GameObjects out of {removableTargets.Length} marked for contextual removal under root object '{root.name}' in scene '{scene.name}'.");
        }

        if (numRemoveTargets == 0)
            _logger.Log($"No GameObjects marked for contextual removal in scene '{scene.name}'.");
    }
}
