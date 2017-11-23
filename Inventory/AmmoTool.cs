using Danware.Unity.Input;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Tool))]
    public class AmmoTool : MonoBehaviour {
        // ABSTRACT DATA TYPES
        /// <summary>
        /// Type arguments are (int oldClipAmmo, int oldBackupAmmo, int newClipAmmo, int newBackupAmmo)
        /// </summary>
        [Serializable]
        public class AmmoEvent : UnityEvent<int, int, int, int> { }

        private Tool _tool;

        // INSPECTOR FIELDS
        public AmmoToolInfo Info;
        public StartStopInput ReloadInput;

        // API INTERFACE
        /// <summary>
        /// The amount of ammo currently in the main clip.
        /// </summary>
        public int CurrentClipAmmo { get; private set; }
        /// <summary>
        /// The total amount of ammo in all backup clips.
        /// </summary>
        public int CurrentBackupAmmo { get; private set; }
        /// <summary>
        /// Load this <see cref="AmmoTool"/> with a specified amount of ammo.
        /// </summary>
        /// <param name="ammo">The amount of ammo with which to load the <see cref="AmmoTool"/>.</param>
        /// <returns>The amount of left-over ammo that could not be stored in the <see cref="AmmoTool"/>'s clips.  Will always be >= 0.</returns>
        public int Load(int ammo) {
            Assert.IsTrue(ammo >= 0, $"Cannot load {nameof(AmmoTool)} {transform.parent.name}.{name} with a negative amount of ammo!");
            return doLoad(ammo);
        }
        /// <summary>
        /// Reload this <see cref="AmmoTool"/>'s current clip from its backup ammo
        /// </summary>
        public void ReloadClip() => doReloadClip();

        public AmmoEvent Loaded = new AmmoEvent();
        public AmmoEvent AmmoReduced = new AmmoEvent();

        // EVENT HANDLERS
        private void Awake() {
            Assert.IsNotNull(Info, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(Danware.Unity.Inventory.AmmoToolInfo)}!");
            Assert.IsNotNull(ReloadInput, $"{GetType().Name} {transform.parent?.name}.{name} must be associated with a {nameof(this.ReloadInput)}!");
            Assert.IsTrue(Info.StartingAmmo <= Info.MaxClipAmmo * (Info.MaxBackupClips + 1), $"{GetType().Name} {transform.parent?.name}.{name} was started with {nameof(this.Info.StartingAmmo)} ammo but it can only store a max of {Info.MaxClipAmmo} * ({Info.MaxBackupClips} + 1) = {Info.MaxClipAmmo * (Info.MaxBackupClips + 1)}!");

            // Initialize ammo
            doLoad(Info.StartingAmmo);

            // Register Tool events
            _tool = GetComponent<Tool>();
            _tool.Using.AddListener(() =>
                _tool.Using.Cancel = (CurrentClipAmmo == 0));
            _tool.Used.AddListener(() => {
                int oldClip = CurrentClipAmmo;
                CurrentClipAmmo = CurrentClipAmmo - 1;
                AmmoReduced.Invoke(oldClip, CurrentBackupAmmo, CurrentClipAmmo, CurrentBackupAmmo);
            });
        }
        private void Update() {
            if (ReloadInput?.Started() ?? false)
                doReloadClip();
        }

        // HELPERS
        private void doReloadClip() {
            // Fill the current clip as much as possible from backup ammo
            int oldClip = CurrentClipAmmo;
            int neededAmmo = Mathf.Clamp(Info.MaxClipAmmo - CurrentClipAmmo, 0, CurrentBackupAmmo);
            CurrentClipAmmo += neededAmmo;
            CurrentBackupAmmo -= neededAmmo;

            // Raise the Reloaded event
            if (CurrentClipAmmo != oldClip)
                Loaded.Invoke(oldClip, CurrentBackupAmmo, CurrentClipAmmo, CurrentBackupAmmo);
        }
        private int doLoad(int ammo) {
            int oldClip = CurrentClipAmmo;
            int oldBackup = CurrentBackupAmmo;

            // Fill the current clip as much as possible
            if (CurrentClipAmmo < Info.MaxClipAmmo) {
                int usableAmmo = Mathf.Min(Info.MaxClipAmmo - CurrentClipAmmo, ammo);
                CurrentClipAmmo += usableAmmo;
                ammo -= usableAmmo;
            }

            // Fill the backup ammo as much as possible
            if (CurrentBackupAmmo < Info.MaxClipAmmo * Info.MaxBackupClips) {
                int usableAmmo = Mathf.Min(Info.MaxBackupClips * Info.MaxClipAmmo - CurrentBackupAmmo, ammo);
                CurrentBackupAmmo += usableAmmo;
                ammo -= usableAmmo;
            }

            // Raise the Reloaded event, if ammo was actually used
            if (CurrentClipAmmo != oldClip || CurrentBackupAmmo != oldBackup)
                Loaded.Invoke(oldClip, oldBackup, CurrentClipAmmo, CurrentBackupAmmo);

            // Return how much ammo was left over
            return ammo;
        }

    }

}
