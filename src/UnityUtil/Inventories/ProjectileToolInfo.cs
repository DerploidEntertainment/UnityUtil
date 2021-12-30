using Sirenix.OdinInspector;

namespace UnityEngine.Inventories {

    [CreateAssetMenu(fileName = "projectile-tool", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inventories)}/{nameof(UnityEngine.Inventories.ProjectileToolInfo)}")]
    public class ProjectileToolInfo : ScriptableObject
    {
        [Required]
        public GameObject? ProjectilePrefab;

        private const string TOOLTIP_LOCAL_SPACE = $"The {nameof(ProjectilePrefab)} will be instantiated at this position in the {nameof(Tool)}'s local space.";

        [Tooltip(TOOLTIP_LOCAL_SPACE)]
        public Vector3 SpawnPosition = Vector3.forward;

        [Tooltip(TOOLTIP_LOCAL_SPACE)]
        public Vector3 SpawnRotation = Vector3.zero;

        [Tooltip(TOOLTIP_LOCAL_SPACE + $" If the root GameObject of the prefab does not have a Rigidbody, then this value will be ignored.")]
        public Vector3 InitialVelocity = Vector3.forward;

    }

}
