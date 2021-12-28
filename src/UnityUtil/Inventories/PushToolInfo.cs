using System;

namespace UnityEngine.Inventories {

    [CreateAssetMenu(fileName = "push-tool", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inventories)}/{nameof(UnityEngine.Inventories.PushToolInfo)}")]
    public class PushToolInfo : ScriptableObject
    {
        public float PushForce = 1f;

        [Tooltip(
            $"If true, then only the closest Rigidbody attacked by this {nameof(UnityEngine.Inventories.Tool)} will be pushed. " +
            "If false, then all attacked Rigidbodies will be pushed."
        )]
        public bool OnlyPushClosest = true;

        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags = Array.Empty<string>();

    }

}
