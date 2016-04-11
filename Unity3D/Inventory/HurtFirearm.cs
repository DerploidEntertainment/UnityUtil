using UnityEngine;
using System.Linq;

namespace Danware.Unity3D.Inventory {

    [RequireComponent(typeof(Firearm))]
    public class HurtFirearm : MonoBehaviour {
        // HIDDEN FIELDS
        private Firearm _firearm;

        // INSPECTOR FIELDS
        public float Damage = 10f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;

        // EVENT HANDLERS
        private void Awake() {
            _firearm = GetComponent<Firearm>();

            _firearm.Fired += handleFired;
            _firearm.AffectingTarget += handleTarget;
        }
        private void handleFired(object sender, Firearm.FireEventArgs e) {
            // Narrow this list down to those targets with Health components
            RaycastHit[] hits = (from h in e.Hits
                                 where h.collider.GetComponent<Health>() != null
                                 where !h.collider.CompareTag("Player")
                                 select h).ToArray();
            if (hits.Count() > 0)
                e.TargetsToAffect.Add(hits[0]);
        }
        private void handleTarget(object sender, Firearm.HitEventArgs e) {
            // Damage the target, if it has a Health component
            RaycastHit hit = e.Hit;
            Health h = hit.collider.GetComponent<Health>();
            if (h != null)
                h.Damage(Damage, HealthChangeMode);
        }

    }

}
