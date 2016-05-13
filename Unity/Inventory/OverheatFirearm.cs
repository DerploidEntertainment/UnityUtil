using UnityEngine;
using System;

namespace Danware.Unity.Inventory {
    
    public class OverheatFirearm : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public class HeatChangedEventArgs : EventArgs {
            public OverheatFirearm Firearm;
            public float OldHeat;
            public float NewHeat;
        }
        public class OverheatChangedEventArgs : EventArgs {
            public OverheatFirearm Firearm;
            public bool Overheated;
        }

        // HIDDEN FIELDS
        private EventHandler<Firearm.FirearmEventArgs> _misfiredInvoker;
        private EventHandler<HeatChangedEventArgs> _heatInvoker;
        private EventHandler<OverheatChangedEventArgs> _overheatInvoker;
        private bool _canCool = true;

        // INSPECTOR FIELDS
        public Firearm Firearm;
        public float CurrentHeat = 0f;
        public float HeatPerShot = 50f;
        public bool AbsoluteHeat = true;
        public float CoolRate = 75f;  // Heat/sec or fraction/sec
        public float MaxHeat = 100f;
        public float OverheatDuration = 2f;
        public event EventHandler<Firearm.FirearmEventArgs> Misfired {
            add { _misfiredInvoker += value; }
            remove { _misfiredInvoker -= value; }
        }
        public event EventHandler<HeatChangedEventArgs> HeatIncreased {
            add { _heatInvoker += value; }
            remove { _heatInvoker -= value; }
        }
        public event EventHandler<OverheatChangedEventArgs> OverheatStateChanged {
            add { _overheatInvoker += value; }
            remove { _overheatInvoker -= value; }
        }

        // API INTERFACE
        public bool OverHeated { get; private set; }

        // EVENT HANDLERS
        private void Awake() {
            Firearm.Firing += handleFiring;
            Firearm.Fired += handleFired;
        }
        private void Update() {
            // Only do cooling updates if they are allowed and necessary
            if (!_canCool || CurrentHeat == 0f)
                return;

            // Cool the Firearm
            float old = CurrentHeat;
            float coolAmt = CoolRate * (AbsoluteHeat ? 1f : MaxHeat) * Time.deltaTime;
            CurrentHeat = Mathf.Max(0f, CurrentHeat - coolAmt);

            // If the Firearm has cooled below the threshold, then raise the Overheat Changed event
            if (OverHeated && CurrentHeat < MaxHeat) {
                OverHeated = false;

                // Raise the Overheat Changed event
                OverheatChangedEventArgs args = new OverheatChangedEventArgs() {
                    Firearm = this,
                    Overheated = OverHeated,
                };
                _overheatInvoker?.Invoke(this, args);
            }
        }
        private void handleFiring(object sender, Firearm.CancelEventArgs e) {
            if (e.Cancel)
                return;

            // If the player has overheated this Firearm, then raise the Misfired event and cancel further firing
            if (OverHeated) {
                Firearm.FirearmEventArgs fireArgs = new Firearm.FirearmEventArgs() { Firearm = Firearm };
                _misfiredInvoker?.Invoke(this, fireArgs);
                e.Cancel = true;
            }

            // Otherwise...
            else {
                // Increase heat level
                float old = CurrentHeat;
                float heatAmt = HeatPerShot * (AbsoluteHeat ? 1f : MaxHeat);
                CurrentHeat += heatAmt;

                // Raise the HeatChanged event
                HeatChangedEventArgs heatArgs = new HeatChangedEventArgs() {
                    Firearm = this,
                    OldHeat = old,
                    NewHeat = CurrentHeat,
                };
                _heatInvoker?.Invoke(this, heatArgs);
            }
        }
        private void handleFired(object sender, Firearm.FireEventArgs e) {
            // If the Firearm hasn't overheated then just return
            if (CurrentHeat < MaxHeat)
                return;

            // Otherwise, block firing for the set amount of time...
            OverHeated = true;
            _canCool = false;
            Invoke(nameof(allowCool), OverheatDuration);

            // ...and raise the Overheat Changed event
            OverheatChangedEventArgs overheatArgs = new OverheatChangedEventArgs() {
                Firearm = this,
                Overheated = OverHeated,
            };
            _overheatInvoker?.Invoke(this, overheatArgs);
        }

        // HELPER FUNCTIONS
        private void allowCool() {
            _canCool = true;
        }

    }

}
