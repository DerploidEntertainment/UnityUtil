using UnityEngine;
using UnityEngine.Assertions;

namespace UnityUtil.Inventory {

    public class HealthCollector : MonoBehaviour {

        private SphereCollider _sphere;

        // INSPECTOR FIELDS
        public Health Health;
        public float Radius = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Health, this.GetAssociationAssertion(nameof(this.Health)));

            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;
        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            // If no collectible was found then just return
            HealthCollectible h = other.attachedRigidbody.GetComponent<HealthCollectible>();
            if (h == null)
                return;

            // If one was found, then adjust its current health as necessary
            float hp = 0f;
            if (Health.CurrentHealth != Health.MaxHealth) {
                hp = Mathf.Min(Health.MaxHealth - Health.CurrentHealth, h.Health);
                Health.Heal(hp, h.HealthChangeMode);
                h.Health -= hp;
            }

            // Destroy the collectible's GameObject as necessary
            if (
                (h.DestroyMode == CollectibleDestroyMode.WhenUsed && hp > 0f) ||
                (h.DestroyMode == CollectibleDestroyMode.WhenEmptied && h.Health == 0f) ||
                (h.DestroyMode == CollectibleDestroyMode.WhenDetected)) {
                Destroy(h.Root);
            }
        }
    }

}
