using UnityEngine;
using U = UnityEngine;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Weapon))]
    public class ProjectileWeapon : MonoBehaviour {
        // INSPECTOR FIELDS
        private Weapon _weapon;
        public Transform ProjectilePrefab;
        public Vector3 RelativeSpawnPosition = Vector3.forward;
        public Vector3 RelativeSpawnRotation = Vector3.zero;
        public float InitialSpeed = 0f;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked += handleFiring;
        }

        // HELPER FUNCTIONS
        private void handleFiring(object sender, Weapon.AttackEventArgs e) {
            // If no Projectile prefab was defined then just return
            if (ProjectilePrefab == null)
                return;

            // Instantiate the Projectile
            Vector3 pos = transform.TransformPoint(RelativeSpawnPosition);
            Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles + RelativeSpawnRotation);
            U.Object obj = Instantiate(ProjectilePrefab, pos, rot);
            Transform projectile = (obj is Transform) ? obj as Transform : (obj as GameObject).transform;

            // Propel the Projectile forward, if requested
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(InitialSpeed * projectile.forward, ForceMode.VelocityChange);
        }

    }

}
