using UnityEngine;

using System;

namespace Danware.Unity3D {

    // EVENTS
    public class DetonatorEventArgs : EventArgs {
        public Detonator Detonator;
    }
    public class DetonatorCancelEventArgs : DetonatorEventArgs {
        public bool Cancel;
    }

    public class Detonator : MonoBehaviour {
        // HIDDEN FIELDS
        private EventHandler<DetonatorCancelEventArgs> _detonatingInvoker;
        private EventHandler<DetonatorEventArgs> _detonatedInvoker;
        private AudioSource _audio;

        // INSPECTOR FIELDS
        public AudioClip ExplosionClip;
        public float ExplosionForce = 10f;
        public float ExplosionRadius = 4f;
        public float ExplosionUpwardsModifier = 0f;
        public float MaxHealthDamage;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        public LayerMask DamageLayer;
        public bool DestroyOnDetonate = true;
        public event EventHandler<DetonatorCancelEventArgs> Detonating {
            add { _detonatingInvoker += value; }
            remove { _detonatingInvoker -= value; }
        }
        public event EventHandler<DetonatorEventArgs> Detonated {
            add { _detonatedInvoker += value; }
            remove { _detonatedInvoker -= value; }
        }

        // EVENT HANDLERS
        private void Awake() {
            // Initialize audio
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.clip = ExplosionClip;
            _audio.loop = false;
            _audio.spatialBlend = 1f;
        }
        public void Detonate() {
            // Raise the Detonated event, allowing listeners to cancel detonation
            DetonatorCancelEventArgs detonatingArgs = new DetonatorCancelEventArgs() {
                Detonator = this,
                Cancel = false,
            };
            _detonatingInvoker?.Invoke(this, detonatingArgs);
            if (detonatingArgs.Cancel)
                return;

            // Apply forces to and damage objects within the Explosion radius
            Collider[] colls = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider c in colls)
                affect(c.gameObject);

            // Make the explosion sound effect
            _audio.clip = ExplosionClip;
            _audio.loop = false;
            _audio.Play();

            // Destroy this object, if requested
            if (DestroyOnDetonate)
                Destroy(this.gameObject);

            // Raise the Detonated event
            DetonatorEventArgs detonatedArgs = new DetonatorEventArgs() {
                Detonator = this
            };
            _detonatedInvoker?.Invoke(this, detonatedArgs);
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
