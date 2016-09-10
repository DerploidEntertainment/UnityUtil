using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    public class HurtWeapon : MonoBehaviour {
        // INSPECTOR FIELDS
        public Weapon Weapon;
        public float Damage = 10f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            Debug.Assert(Weapon != null, $"{nameof(HurtWeapon)} {name} was not associated with a {nameof(Weapon)}!");
            Weapon.Attacked += Weapon_Attacked;
        }
        private void Weapon_Attacked(object sender, Weapon.AttackEventArgs e) {
            // Narrow this list down to those targets with Health components
            RaycastHit[] hits = (from h in e.Hits
                                 where h.collider.GetComponent<Health>() != null
                                 where !h.collider.CompareTag("Player")
                                 select h).ToArray();
            if (hits.Count() > 0) {
                Weapon.TargetData td = new Weapon.TargetData();
                td.Callback += affectTarget;
                e.Add(hits[0], td);
            }

        }
        private void affectTarget(RaycastHit hit) {
            // Damage the target, if it has a Health component
            Health h = hit.collider.GetComponent<Health>();
            if (h != null)
                h.Damage(Damage, HealthChangeMode);
        }

    }

}
