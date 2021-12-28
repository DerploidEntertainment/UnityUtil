using Sirenix.OdinInspector;
using System;

namespace UnityEngine
{
    [Flags]
    public enum BuildContext
    {
        PlayMode = 0b001,
        DebugBuilds = 0b010,
        ReleaseBuilds = 0b100,
        AllBuilds = 0b111,
    }

    public enum DestroyBehavior
    {
        SelfAndChildren,
        ChildrenOnly,
    }
    public class RemoveFromBuild : MonoBehaviour
    {
        private const string MSG_PRESERVE_CONTEXT =
            "This GameObject (and/or its children) will be removed from the build unless the build context matches " +
            $"{nameof(PreserveBuildContexts)} AND the platform matches {nameof(PreservePlatforms)}.";

        [InfoBox("This component will not actually affect your scene files; it will only remove this GameObject and/or its children from BUILDS.")]
        [InfoBox(
            "This component will NOT affect prefabs instantiated at runtime. " +
            $"Any GameObjects with {nameof(RemoveFromBuild)} components in prefabs will NOT be removed at runtime unless YOU do so explicitly.",
            InfoMessageType = InfoMessageType.Warning
        )]

        [Tooltip("What should be destroyed during builds?")]
        public DestroyBehavior DestroyBehavior = DestroyBehavior.SelfAndChildren;

        [Tooltip(MSG_PRESERVE_CONTEXT)]
        public BuildContext PreserveBuildContexts;

        [Tooltip(MSG_PRESERVE_CONTEXT + " Order does not matter.")]
        public RuntimePlatform[] PreservePlatforms = Array.Empty<RuntimePlatform>();
    }
}
