using System;
using UnityEngine;

namespace Danware.Unity.Inventory {

    public class HealthCollector : DetectorResponder {

        // INSPECTOR FIELDS
        public Health Health;
        public EnterDetector[] Detectors;

        // EVENT HANDLERS
        public void Awake() {
            foreach (EnterDetector detector in Detectors)
                detector.Detected += Detector_Detected;
        }
        protected override void Detector_Detected(object sender, ColliderDetectedEventArgs e) {
            // If we are not associated with a Health, or if the target is not a HealthCollectible, then just early exit
            if (Health == null) {
                Debug.LogWarning($"{nameof(HealthCollector)} could not collect a {nameof(HealthCollectible)} because it was not associated with a {nameof(Health)}!");
                return;
            }
            var h = e.Target as HealthCollectible;
            if (h == null)
                return;

            // Otherwise, make sure this HealthCollectible has valid values
            Debug.Assert(h.HealthAmount >= 0, $"{nameof(HealthCollectible)} {name} must have a positive value for {nameof(h.HealthAmount)}!");

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
