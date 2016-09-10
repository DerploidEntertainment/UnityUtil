using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class AmmoCollectible : Collectible {
        // ABSTRACT DATA TYPES
        public enum DestroyModeType {
            Never,
            WhenCollected,
            WhenAmmoEmptied,
            WhenAmmoUsed,
        }

        // INSPECTOR FIELDS
        public int AmmoAmount;
        public string FirearmTypeID;
        public DestroyModeType DestroyMode = DestroyModeType.WhenAmmoUsed;

        // HELPER FUNCTIONS
        protected override void doCollect(Transform targetRoot) {
            Debug.Assert(AmmoAmount >= 0, $"{nameof(AmmoCollectible)} {name} must have a positive value for {nameof(AmmoAmount)}!");
            Debug.Assert(FirearmTypeID != "", $"{nameof(AmmoCollectible)} {name} must have a specify a value for {nameof(AmmoAmount)}!");

            // Try to get the target's Firearm component, with the correct typeID
            Weapon weapon = null;
            Weapon[] weapons = targetRoot.GetComponentsInChildren<Weapon>(true);
            foreach (Weapon w in weapons) {
                if (w.TypeID == FirearmTypeID) {
                    weapon = w;
                    break;
                }
            }
            if (weapon == null)
                return;

            // If one was found, then adjust its current ammo as necessary
            int ammo = 0;
            int currAmmo = weapon.BackupAmmo + weapon.CurrentClipAmmo;
            int maxAmmo = weapon.MaxClips * weapon.MaxClipAmmo;
            if (currAmmo != maxAmmo) {
                ammo = Mathf.Min(maxAmmo - currAmmo, AmmoAmount);
                weapon.Load(ammo);
                AmmoAmount -= ammo;
            }

            // Destroy this collectible's GameObject as necessary
            if ((DestroyMode == DestroyModeType.WhenCollected) ||
                (DestroyMode == DestroyModeType.WhenAmmoUsed && ammo > 0f) ||
                (DestroyMode == DestroyModeType.WhenAmmoEmptied && AmmoAmount == 0f))
            {
                Destroy(gameObject);
                Destroy(PhysicalObject.gameObject);
            }

        }
    }

}
