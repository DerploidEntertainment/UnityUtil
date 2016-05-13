using UnityEngine;
using System;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {
    
    public class ClipAmmoFirearm : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class AmmoChangedEventArgs : EventArgs {
            public ClipAmmoFirearm Firearm;
            public int OldClipAmmo;
            public int OldBackupAmmo;
            public int NewClipAmmo;
            public int NewBackupAmmo;
        }

        // HIDDEN FIELDS
        private EventHandler<Firearm.FirearmEventArgs> _misfiredInvoker;
        private EventHandler<AmmoChangedEventArgs> _ammoInvoker;

        // INSPECTOR FIELDS
        public Firearm Firearm;
        public int CurrentClipAmmo;
        public int MaxClipAmmo;
        public int MaxClips;
        public int BackupAmmo;
        public event EventHandler<Firearm.FirearmEventArgs> Misfired {
            add { _misfiredInvoker += value; }
            remove { _misfiredInvoker -= value; }
        }
        public event EventHandler<AmmoChangedEventArgs> ClipAmmoChanged {
            add { _ammoInvoker += value; }
            remove { _ammoInvoker -= value; }
        }

        // API INTERFACE
        public static StartStopInput ReloadInput { get; set; }
        public int TotalCurrentAmmo => CurrentClipAmmo + BackupAmmo;
        public int TotalMaxAmmo => MaxClipAmmo * MaxClips;
        public void Load(int ammo) {
            Debug.AssertFormat(ammo >= 0, "Tried to reload Firearm {0} with a negative amount of ammo!", this.name);
            doLoad(ammo);
        }
        public void ReloadClip() {
            doReloadClip();
        }


        // EVENT HANDLERS
        private void Awake() {
            Firearm.Firing += handleFiring;
        }
        private void Update() {
            // Reload if the player requested it
            bool reloaded = ReloadInput.Started;
            if (reloaded)
                doReloadClip();
        }

        // HELPER FUNCTIONS
        private void doLoad(int ammo) {
            // Determine how much ammo is needed to fill the current clip and the backup
            int tempAmmo = ammo;
            int oldClip = CurrentClipAmmo;
            int oldBackup = BackupAmmo;

            // Fill the current clip as much as possible
            int clipFill = MaxClipAmmo - CurrentClipAmmo;
            if (tempAmmo < clipFill)
                CurrentClipAmmo += tempAmmo;
            else {
                CurrentClipAmmo += clipFill;
                tempAmmo -= clipFill;
            }

            // Fill the backup ammo as much as possible
            // Any remaining ammo is ignored
            int backupFill = MaxBackupAmmo - BackupAmmo;
            if (tempAmmo < backupFill)
                BackupAmmo += tempAmmo;
            else {
                BackupAmmo += backupFill;
                tempAmmo -= backupFill;
            }

            // Raise the ClipAmmoIncreased event
            AmmoChangedEventArgs args = new AmmoChangedEventArgs() {
                Firearm = this,
                OldClipAmmo = oldClip,
                OldBackupAmmo = oldBackup,
                NewClipAmmo = CurrentClipAmmo,
                NewBackupAmmo = BackupAmmo,
            };
            _ammoInvoker?.Invoke(this, args);
        }
        private void doReloadClip() {
            // Fill the current clip as much as possible from backup ammo
            int old = CurrentClipAmmo;
            int clipFill = MaxClipAmmo - CurrentClipAmmo;
            int availableAmmo = Mathf.Min(BackupAmmo, clipFill);
            CurrentClipAmmo += availableAmmo;
            BackupAmmo -= availableAmmo;

            // If the clip was actually reloaded, raise the ClipAmmoIncreased event
            if (availableAmmo > 0) {
                AmmoChangedEventArgs args = new AmmoChangedEventArgs() {
                    Firearm = this,
                    OldClipAmmo = old,
                    NewClipAmmo = CurrentClipAmmo,
                };
                _ammoInvoker?.Invoke(this, args);
            }
        }
        private void handleFiring(object sender, Firearm.CancelEventArgs e) {
            if (e.Cancel)
                return;

            // If the player has no ammo in their clip, then raise the Misfired event and cancel further firing
            if (CurrentClipAmmo == 0) {
                Firearm.FirearmEventArgs fireArgs = new Firearm.FirearmEventArgs() { Firearm = Firearm };
                _misfiredInvoker?.Invoke(this, fireArgs);
                e.Cancel = true;
            }

            // Otherwise...
            else {
                // Reduce current clip ammo
                int old = CurrentClipAmmo;
                --CurrentClipAmmo;

                // Raise the ClipAmmoDecreased event
                AmmoChangedEventArgs ammoArgs = new AmmoChangedEventArgs() {
                    Firearm = this,
                    OldClipAmmo = old,
                    NewClipAmmo = CurrentClipAmmo,
                };
                _ammoInvoker?.Invoke(this, ammoArgs);
            }
        }
        private int MaxBackupAmmo {
            get { return MaxClipAmmo * (MaxClips - 1); }
        }

    }

}
