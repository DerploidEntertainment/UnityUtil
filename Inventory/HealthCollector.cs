using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Inventory {

    public class HealthCollector : MonoBehaviour {

        private SphereCollider _sphere;

        // INSPECTOR FIELDS
        public Health Health;
        public float Radius = 1f;

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Health, $"{nameof(HealthCollector)} {transform.parent.name}.{name} must be associated with a {nameof(this.Health)}!");

            _sphere = gameObject.AddComponent<SphereCollider>();
            _sphere.radius = Radius;
            _sphere.isTrigger = true;

        }
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, Radius);
        private void OnTriggerEnter(Collider other) {
            // If no AmmoCollectible was found then just return
            HealthCollectible h = other.attachedRigidbody.GetComponent<HealthCollectible>();
            if (h == null)
                return;

            // If one was found, then adjust its current health as necessary
            float hp = 0f;
            if (Health.CurrentHealth != Health.MaxHealth) {
                hp = Mathf.Min(Health.MaxHealth - Health.CurrentHealth, h.HealthAmount);
                Health.Heal(hp, h.HealthChangeMode);
                h.HealthAmount -= hp;
            }

            // Destroy this collectible's GameObject as necessary
            bool collectDestroy = (h.DestroyMode == CollectibleDestroyMode.WhenDetected);
            bool useDestroy = (h.DestroyMode == CollectibleDestroyMode.WhenUsed && hp > 0f);
            bool emptyDestroy = (h.DestroyMode == CollectibleDestroyMode.WhenEmptied && h.HealthAmount == 0f);
            if (collectDestroy || useDestroy || emptyDestroy)
                Destroy(h.gameObject);
        }
    }

}
