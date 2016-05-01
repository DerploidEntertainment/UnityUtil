using UnityEngine;

using System;

namespace Danware.Unity3D {

    public class Detonator : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class DetonatorEventArgs : EventArgs {
            public Detonator Detonator;
        }
        public class CancelEventArgs : DetonatorEventArgs {
            public bool Cancel;
        }

        // HIDDEN FIELDS
        private EventHandler<CancelEventArgs> _detonatingInvoker;
        private EventHandler<DetonatorEventArgs> _detonatedInvoker;

        // INSPECTOR FIELDS
        public Transform EffectsPrefab;
        public float ExplosionForce = 10f;
        public float ExplosionRadius = 4f;
        public float ExplosionUpwardsModifier = 2f;
        public float MaxHealthDamage;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        public LayerMask DamageLayer;
        public bool DestroyOnDetonate = true;

        public event EventHandler<CancelEventArgs> Detonating {
            add { _detonatingInvoker += value; }
            remove { _detonatingInvoker -= value; }
        }
        public event EventHandler<DetonatorEventArgs> Detonated {
            add { _detonatedInvoker += value; }
            remove { _detonatedInvoker -= value; }
        }

        // EVENT HANDLERS
        public void Detonate() {
            // Raise the Detonated event, allowing listeners to cancel detonation
            CancelEventArgs detonatingArgs = new CancelEventArgs() {
                Detonator = this,
                Cancel = false,
            };
            _detonatingInvoker?.Invoke(this, detonatingArgs);
            if (detonatingArgs.Cancel)
                return;

            // If we didn't cancel, then affect objects within the Explosion radius
            Collider[] colls = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider c in colls)
                affect(c.gameObject);

            // Instantiate the explosion prefab if one was provided
            if (EffectsPrefab != null)
                Instantiate(EffectsPrefab, transform.position, Quaternion.identity);

            // Raise the Detonated event
            DetonatorEventArgs detonatedArgs = new DetonatorEventArgs() {
                Detonator = this
            };
            _detonatedInvoker?.Invoke(this, detonatedArgs);

            // Destroy this object, if requested
            if (DestroyOnDetonate)
                Destroy(this.gameObject);
        }

        // HELPER FUNCTIONS
        private void affect(GameObject obj) {
            // Apply the explosion force!!!
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Impulse);

            // Damage any Health component (if it matches the DamageLayer)
            // Damage amount decreases with distance from the explosion
            Health h = obj.GetComponent<Health>();
            if (h != null) {
                bool shouldDamage = ((DamageLayer | obj.layer) != 0);
                if (shouldDamage) {
                    float dist = Vector3.Distance(transform.position, obj.transform.position);
                    float hp = MaxHealthDamage * dist / ExplosionRadius;
                    h.Damage(hp, HealthChangeMode);
                }
            }
        }
    }

}
