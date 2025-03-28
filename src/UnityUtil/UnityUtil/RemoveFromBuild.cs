using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UnityUtil;

[Flags]
public enum BuildContext
{
    None = 0b000,
    PlayMode = 0b001,
    DebugBuild = 0b010,
    ReleaseBuild = 0b100,
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

    [InfoBox(
        "This component will not actually change your scene file; rather, it will remove this GameObject and/or its children from the scene during builds." +
        "This keeps your builds smaller and scene load times faster, without adding noise to version control."
    )]
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
    [ListDrawerSettings(DefaultExpandedState = true)]
    public RuntimePlatform[] PreservePlatforms = [];
}
