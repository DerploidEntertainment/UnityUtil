using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class HealthCollectible : Collectible {
        // ABSTRACT DATA TYPES
        public enum DestroyModeType {
            Never,
            WhenCollected,
            WhenHealthEmptied,
            WhenHealthUsed,
        }

        // INSPECTOR FIELDS
        public float HealthAmount = 25f;
        public Health.ChangeMode HealthChangeMode = Health.ChangeMode.Absolute;
        public DestroyModeType DestroyMode = DestroyModeType.WhenHealthUsed;

        // EVENT HANDLERS
        private void Awake() {
            Debug.Assert(HealthAmount >= 0, $"{nameof(HealthCollectible)} {name} must have a positive value for {nameof(HealthAmount)}!");
        }

        // HELPER FUNCTIONS
        protected override void doCollect(Transform targetRoot) {
            // Try to get the target's Health component
            Health h = targetRoot.GetComponentInChildren<Health>();
            if (h == null)
                return;

            // If one was found, then adjust its current health as necessary
            float hp = 0f;
            if (h.CurrentHealth != h.MaxHealth) {
                hp = Mathf.Min(h.MaxHealth - h.CurrentHealth, HealthAmount);
                h.Heal(hp, HealthChangeMode);
                HealthAmount -= hp;
            }

            // Destroy this collectible's GameObject as necessary
            if ((DestroyMode == DestroyModeType.WhenCollected) ||
                (DestroyMode == DestroyModeType.WhenHealthUsed && hp > 0f) ||
                (DestroyMode == DestroyModeType.WhenHealthEmptied && HealthAmount == 0f))
            {
                Destroy(gameObject);
                Destroy(PhysicalObject.gameObject);
            }

        }
    }

}
