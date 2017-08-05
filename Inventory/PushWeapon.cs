using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Weapon))]
    public class PushWeapon : MonoBehaviour {

        // HIDDEN FIELDS
        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float AttackForce = 1f;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked += weapon_Attacked;
        }
        private void weapon_Attacked(object sender, Weapon.AttackEventArgs e) {
            // Narrow this list down to those targets with Rigidbody components
            RaycastHit[] hits = e.Hits.Where(h => h.collider.attachedRigidbody != null).ToArray();
            if (hits.Count() > 0) {
                var td = new Weapon.TargetData();
                td.Callback += affectTarget;
                e.Add(hits[0], td);
            }
        }
        private void affectTarget(RaycastHit hit) {
            // Apply a force to the target, if it has a Rigidbody component
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null)
                rb.AddForceAtPosition(AttackForce * transform.forward, hit.point, ForceMode.Impulse);
        }

    }

}
