namespace UnityEngine.Inventory {

    [CreateAssetMenu(fileName = "projectile-tool", menuName = nameof(UnityEngine.Inventory) + "/" + nameof(UnityEngine.Inventory.ProjectileToolInfo))]
    public class ProjectileToolInfo : ScriptableObject {

        public GameObject ProjectilePrefab;
        [Tooltip("The " + nameof(ProjectileToolInfo.ProjectilePrefab) + " will be instantiated at this position in the " + nameof(UnityEngine.Inventory.Tool) + "'s local space.")]
        public Vector3 SpawnPosition = Vector3.forward;
        [Tooltip("The " + nameof(ProjectileToolInfo.ProjectilePrefab) + " will be instantiated with this rotation in the " + nameof(UnityEngine.Inventory.Tool) + "'s local space.")]
        public Vector3 SpawnRotation = Vector3.zero;
        [Tooltip("The " + nameof(ProjectileToolInfo.ProjectilePrefab) + " will be instantiated with this initial velocity in the " + nameof(UnityEngine.Inventory.Tool) + "'s local space.  If the root GameObject of the prefab does not have a Rigidbody, then this value will be ignored.")]
        public Vector3 InitialVelocity = Vector3.forward;

    }

}
