﻿using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Weapon))]
    public class HurtWeapon : MonoBehaviour {

        // HIDDEN FIELDS
        private Weapon _weapon;

        // INSPECTOR FIELDS
        public float Damage = 10f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            _weapon = GetComponent<Weapon>();
            _weapon.Attacked += Weapon_Attacked;
        }
        private void Weapon_Attacked(object sender, Weapon.AttackEventArgs e) {
            // Narrow this list down to those targets with Health components
            RaycastHit[] hits = (from h in e.Hits
                                 where h.collider.GetComponent<PhysTarget>().TargetComponent as Health != null
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
            PhysTarget pt = hit.collider.GetComponent<PhysTarget>();
            Health h = pt.TargetComponent as Health;
            if (h != null)
                h.Damage(Damage, HealthChangeMode);
        }

    }

}