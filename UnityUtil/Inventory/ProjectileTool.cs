using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Tool))]
    public class ProjectileTool : MonoBehaviour {

        private Tool _tool;

        // INSPECTOR FIELDS
        public ProjectileToolInfo Info;
        [Tooltip("The ProjectilePrefab will be parented to this Transform after it is instantiated.")]
        public Transform ProjectileParent;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, this.GetAssociationAssertion(nameof(UnityEngine.Inventory.ProjectileToolInfo)));

            _tool = GetComponent<Tool>();
            _tool.Used.AddListener(spawnProjectile);
        }

        // HELPER FUNCTIONS
        private void spawnProjectile() {
            // Instantiate the Projectile
            Vector3 pos = transform.TransformPoint(Info.SpawnPosition);
            Quaternion rot = transform.rotation * Quaternion.Euler(Info.SpawnRotation);
            GameObject projectile = Instantiate(Info.ProjectilePrefab, pos, rot, ProjectileParent);

            // Propel the Projectile forward, if requested/possible
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(transform.TransformDirection(Info.InitialVelocity), ForceMode.VelocityChange);
        }

    }

}
