using UnityEngine;

namespace Danware.Unity.Inventory {

    public class PushToolInfo : ScriptableObject {

        // INSPECTOR FIELDS
        public float PushForce = 1f;
        [Tooltip("If true, then only the closest Rigidbody attacked by this Weapon will be pushed.  If false, then all attacked Rigidbodies will be pushed.")]
        public bool OnlyPushClosest = true;
        [Tooltip("If a Collider has any of these tags, then it will be ignored, allowing Colliders inside/behind it to be affected.")]
        public string[] IgnoreColliderTags;

    }

}
