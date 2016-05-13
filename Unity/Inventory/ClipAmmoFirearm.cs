﻿using UnityEngine;
using System;

using Danware.Unity.Input;

namespace Danware.Unity.Inventory {
    
    public class ClipAmmoFirearm : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class AmmoChangedEventArgs : EventArgs {
            public ClipAmmoFirearm Firearm;
            public int OldAmmo;
            public int NewAmmo;
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
            // Fill the current clip as much as possible
            int tempAmmo = ammo;
            int old = CurrentClipAmmo;
            int clipFill = MaxClipAmmo - CurrentClipAmmo;
            if (tempAmmo <= clipFill)
                CurrentClipAmmo += tempAmmo;

            // Fill backup ammo as much as possible
            else {
                CurrentClipAmmo += clipFill;
                tempAmmo -= clipFill;
                int backupFill = MaxBackupAmmo - BackupAmmo;
                BackupAmmo += backupFill;
                tempAmmo -= backupFill;
            }

            // Raise the ClipAmmoIncreased event
            AmmoChangedEventArgs args = new AmmoChangedEventArgs() {
                Firearm = this,
                OldAmmo = old,
                NewAmmo = CurrentClipAmmo,
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
                    OldAmmo = old,
                    NewAmmo = CurrentClipAmmo,
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
                    OldAmmo = old,
                    NewAmmo = CurrentClipAmmo,
                };
                _ammoInvoker?.Invoke(this, ammoArgs);
            }
        }
        private int MaxBackupAmmo {
            get { return MaxClipAmmo * (MaxClips - 1); }
        }

    }

}
