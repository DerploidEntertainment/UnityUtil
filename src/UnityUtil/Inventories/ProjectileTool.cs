using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine.Inventories {

    [RequireComponent(typeof(Tool))]
    public class ProjectileTool : MonoBehaviour
    {
        private Tool? _tool;

        [Required] public ProjectileToolInfo? Info;

        [Tooltip("The ProjectilePrefab will be parented to this Transform after it is instantiated.")]
        [Required] public Transform? ProjectileParent;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() {
            _tool = GetComponent<Tool>();
            _tool.Used.AddListener(spawnProjectile);
        }

        private void spawnProjectile() {
            // Instantiate the Projectile
            Vector3 pos = transform.TransformPoint(Info!.SpawnPosition);
            Quaternion rot = transform.rotation * Quaternion.Euler(Info.SpawnRotation);
            GameObject projectile = Instantiate(Info.ProjectilePrefab, pos, rot, ProjectileParent)!;

            // Propel the Projectile forward, if requested/possible
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(transform.TransformDirection(Info.InitialVelocity), ForceMode.VelocityChange);
        }

    }

}
