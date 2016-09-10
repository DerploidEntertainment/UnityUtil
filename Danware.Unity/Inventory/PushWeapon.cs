using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    public class PushWeapon : MonoBehaviour {
        // INSPECTOR FIELDS
        public Weapon Weapon;
        public float AttackForce = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Debug.Assert(Weapon != null, $"{nameof(PushWeapon)} {name} was not associated with a {nameof(Weapon)}!");
            Weapon.Attacked += Weapon_Attacked;
        }
        private void Weapon_Attacked(object sender, Weapon.AttackEventArgs e) {
            // Narrow this list down to those targets with Rigidbody components
            RaycastHit[] hits = e.Hits.Where(h => h.collider.attachedRigidbody != null).ToArray();
            if (hits.Count() > 0) {
                Weapon.TargetData td = new Weapon.TargetData();
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
