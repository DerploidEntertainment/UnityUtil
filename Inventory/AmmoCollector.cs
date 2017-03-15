using UnityEngine;
using System.Linq;

namespace Danware.Unity.Inventory {

    public class AmmoCollector : DetectorResponder {

        // INSPECTOR FIELDS
        public Inventory Inventory;
        public EnterDetector[] Detectors;

        // EVENT HANDLERS
        public void Awake() {
            foreach (EnterDetector detector in Detectors)
                detector.Detected += Detector_Detected;
        }
        protected override void Detector_Detected(object sender, DetectedEventArgs e) {
            // If we are not associated with an Inventory, or if the target is not an AmmoCollectible, then just early exit
            if (Inventory == null) {
                Debug.LogWarning($"{nameof(AmmoCollector)} could not collect a {nameof(AmmoCollectible)} because it was not associated with an {nameof(Inventory)}!");
                return;
            }
            var a = e.Target as AmmoCollectible;
            if (a == null)
                return;

            // Otherwise, make sure this AmmoCollectible has valid values
            Debug.Assert(a.AmmoAmount >= 0, $"{nameof(AmmoCollectible)} {a.name} must have a positive value for {nameof(a.AmmoAmount)}!");
            Debug.Assert(a.WeaponTypeName != "", $"{nameof(AmmoCollectible)} {a.name} must specify a value for {nameof(a.WeaponTypeName)}!");

            // Try to find a Weapon with a matching name in the Inventory
            Weapon weapon = Inventory?.GetComponentsInChildren<Weapon>(true)
                                     ?.SingleOrDefault(w => w.WeaponName == a.WeaponTypeName);
            if (weapon == null)
                return;

            // If one was found, then adjust its current ammo as necessary
            int ammo = 0;
            int currAmmo = weapon.BackupAmmo + weapon.CurrentClipAmmo;
            int maxAmmo = weapon.MaxClips * weapon.MaxClipAmmo;
            if (currAmmo != maxAmmo) {
                ammo = Mathf.Min(maxAmmo - currAmmo, a.AmmoAmount);
                weapon.Load(ammo);
                a.AmmoAmount -= ammo;
            }

            // Destroy the AmmoCollectible's GameObject as necessary
            bool detectDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenDetected);
            bool useDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenUsed && ammo > 0f);
            bool emptyDestroy = (a.DestroyMode == CollectibleDestroyMode.WhenEmptied && a.AmmoAmount == 0f);
            if (detectDestroy || useDestroy || emptyDestroy)
                Destroy(a.gameObject);
        }
    }

}
