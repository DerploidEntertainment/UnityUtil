﻿using UnityEngine;
using System.Linq;

namespace Danware.Unity3D.Inventory {

    [RequireComponent(typeof(Firearm))]
    public class PushFirearm : MonoBehaviour {
        // HIDDEN FIELDS
        private Firearm _firearm;

        // INSPECTOR FIELDS
        public float FireForce = 1f;

        // EVENT HANDLERS
        private void Awake() {
            _firearm = GetComponent<Firearm>();

            _firearm.Fired += handleFired;
        }
        private void handleFired(object sender, Firearm.FireEventArgs e) {
            // Narrow this list down to those targets with Rigidbody components
            RaycastHit[] hits = e.Hits.Where(h => h.collider.GetComponent<Rigidbody>() != null).ToArray();
            if (hits.Count() > 0) {
                Firearm.TargetData td = new Firearm.TargetData();
                td.Callback += affectTarget;
                e.Add(hits[0], td);
            }
        }
        private void affectTarget(RaycastHit hit) {
            // Apply a force to the target, if it has a Rigidbody component
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForceAtPosition(FireForce * transform.forward, hit.point, ForceMode.Impulse);
        }

    }

}
