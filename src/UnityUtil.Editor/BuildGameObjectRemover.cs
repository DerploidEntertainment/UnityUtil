using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Logging;
using UnityEngine.SceneManagement;

namespace UnityUtil.Editor
{
    public class BuildGameObjectRemover
    {
        private readonly ILogger _logger;

        public BuildGameObjectRemover(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

        public void RemoveGameObjectsFromScene(Scene scene, List<RemoveFromBuild> removeTargetBuffer = null)
        {
            RuntimePlatform platform = Application.platform;
            BuildContext buildContext =
                Application.isPlaying ? BuildContext.PlayMode :
                Debug.isDebugBuild ? BuildContext.DebugBuilds :
                BuildContext.ReleaseBuilds;

            int numRemoveTargets = 0;
            removeTargetBuffer ??= new List<RemoveFromBuild>(100);  // This is just a wild-a$$ guess. If consumers want a reasonably sized buffer, they should pass in their own
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; ++r) {
                GameObject root = roots[r];

                root.GetComponentsInChildren(includeInactive: true, removeTargetBuffer);
                int removeTargetsUnderRoot = removeTargetBuffer.Count;
                numRemoveTargets += removeTargetsUnderRoot;

                // Determine which GameObjects should actually be removed
                RemoveFromBuild[] targetsToRemove = removeTargetBuffer
                    .Where(x => !x.PreservePlatforms.Contains(platform) || (x.PreserveBuildContexts & buildContext) == 0)
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

                if (removeTargetsUnderRoot > 0)
                    _logger.Log($"Removed {targetsToRemove.Length} GameObjects under root object '{root.name}' in scene '{scene.name}', out of {removeTargetsUnderRoot} marked for contextual removal.");
            }


            if (numRemoveTargets == 0)
                _logger.Log($"No GameObjects marked for contextual removal in scene '{scene.name}'.");
        }
    }
}
