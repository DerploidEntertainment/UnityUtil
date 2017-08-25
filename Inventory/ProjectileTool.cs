using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Tool))]
    public class ProjectileTool : MonoBehaviour {

        private Tool _tool;

        // INSPECTOR FIELDS
        public GameObject ProjectilePrefab;
        [Tooltip("The ProjectilePrefab will be instantiated at this position in the Tool's local space.")]
        public Vector3 SpawnPosition = Vector3.forward;
        [Tooltip("The ProjectilePrefab will be instantiated with this rotation in the Tool's local space.")]
        public Vector3 SpawnRotation = Vector3.zero;
        [Tooltip("The ProjectilePrefab will be instantiated with this initial velocity in the Tool's local space.  If the root GameObject of the prefab does not have a Rigidbody, then this value will be ignored.")]
        public Vector3 InitialVelocity = Vector3.forward;
        [Tooltip("The ProjectilePrefab will be parented to this Transform after it is instantiated.")]
        public Transform ProjectileParent;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(ProjectilePrefab, $"{nameof(ProjectileTool)} {transform.parent.name}.{name} must be associated with a {nameof(this.ProjectilePrefab)}!");

            _tool = GetComponent<Tool>();
            _tool.Used.AddListener(spawnProjectile);
        }

        // HELPER FUNCTIONS
        private void spawnProjectile() {
            // Instantiate the Projectile
            Vector3 pos = transform.TransformPoint(SpawnPosition);
            Quaternion rot = transform.rotation * Quaternion.Euler(SpawnRotation);
            GameObject projectile = Instantiate(ProjectilePrefab, pos, rot, ProjectileParent);

            // Propel the Projectile forward, if requested/possible
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(transform.TransformDirection(InitialVelocity), ForceMode.VelocityChange);
        }

    }

}
