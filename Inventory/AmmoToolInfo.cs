using UnityEngine;

namespace Danware.Unity.Inventory {

    public class AmmoToolInfo : ScriptableObject {

        [Tooltip("A case-insensitive string to identify different types of ammo for collecting (e.g., 'Pistol').")]
        public string AmmoTypeName = "Gun";
        [Tooltip("The maximum ammo that can be in a clip.")]
        public int MaxClipAmmo;
        [Tooltip("The maximum number of backup clips.  These clips may or may not all be filled.")]
        public int MaxBackupClips;
        [Tooltip("The amount of ammo that this Tool has when instantiated.  Must be <= MaxClipAmmo * (MaxBackupClips + 1).")]
        public int StartingAmmo;

    }

}
