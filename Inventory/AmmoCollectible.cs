using UnityEngine;

using System.Linq;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Collectible))]
    public class AmmoCollectible : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public enum DestroyModeType {
            Never,
            WhenCollected,
            WhenAmmoEmptied,
            WhenAmmoUsed,
        }

        // HIDDEN FIELDS
        private Collectible _collectible;

        // INSPECTOR FIELDS
        [Tooltip("If there is a GameObject that gives this Collectible a physical representation, then reference it here.")]
        public GameObject PhysicalObject;
        public int AmmoAmount;
        public string WeaponTypeName;
        public DestroyModeType DestroyMode = DestroyModeType.WhenAmmoUsed;

        // EVENT HANDLERS
        private void Awake() {
            _collectible = GetComponent<Collectible>();
            _collectible.Collected += (sender, e) => doCollect(e.TargetRoot);
        }

        // HELPERS
        private void doCollect(Transform targetRoot) {
            Debug.Assert(AmmoAmount >= 0, $"{nameof(AmmoCollectible)} {name} must have a positive value for {nameof(AmmoAmount)}!");
            Debug.Assert(WeaponTypeName != "", $"{nameof(AmmoCollectible)} {name} must have a specify a value for {nameof(AmmoAmount)}!");

            // Try to get the target's Weapon component, with the correct typeID
            Weapon weapon = targetRoot.GetComponentsInChildren<Weapon>(true)
                                      .SingleOrDefault(w => w.WeaponName == WeaponTypeName);
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
