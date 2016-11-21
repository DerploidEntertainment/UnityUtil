using UnityEngine;
using System;

namespace Danware.Unity {

    public class Health : MonoBehaviour {
        // ABSTRACT DATA TYPES
        public enum ChangeMode {
            Absolute,
            PercentCurrent,
            PercentMax,
        }
        public class HealthEventArgs : EventArgs {
            public HealthEventArgs(Health health) {
                Health = health;
            }
            public Health Health { get; }
        }
        public class ChangedEventArgs : HealthEventArgs {
            public ChangedEventArgs(Health health, float oldValue, float newValue) : base(health) {
                OldValue = oldValue;
                NewValue = newValue;
            }
            public float OldValue;
            public float NewValue;
        }

        // HIDDEN FIELDS
        private EventHandler<ChangedEventArgs> _healthInvoker;

        // INSPECTOR FIELDS
        public float CurrentHealth;
        public float MaxHealth;
        public event EventHandler<ChangedEventArgs> HealthChanged {
            add { _healthInvoker += value; }
            remove { _healthInvoker -= value; }
        }

        // API INTERFACE
        public void Heal(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            Debug.AssertFormat(amount >= 0, "Tried to heal Health {0} by a negative amount!", name);
            doHeal(amount, changeMode);
        }
        public void HealCompletely() {
            doHeal(MaxHealth - CurrentHealth, ChangeMode.Absolute);
        }
        public void Damage(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            Debug.AssertFormat(amount >= 0, "Tried to wound Health {0} by a negative amount!", name);
            doDamage(amount, changeMode);
        }
        public void Kill() {
            doDamage(CurrentHealth, ChangeMode.Absolute);
        }

        // HELPER FUNCTIONS
        private void doHeal(float amount, ChangeMode changeMode) {
            // Raise the Current Health
            float old = CurrentHealth;
            float hp = hpFromAmount(amount, changeMode);
            CurrentHealth = Mathf.Min(old + hp, MaxHealth);

            // Raise the HealthChanged event, if a change actually occurred
            if (CurrentHealth != old) {
                ChangedEventArgs args = new ChangedEventArgs(this, old, CurrentHealth);
                _healthInvoker?.Invoke(this, args);
            }
        }
        private void doDamage(float amount, ChangeMode changeMode) {
            // Lower the Current Health
            float old = CurrentHealth;
            float hp = hpFromAmount(amount, changeMode);
            CurrentHealth = Mathf.Max(old - hp, 0f);

            // Raise the HealthChanged event, if a change actually occurred
            if (CurrentHealth != old) {
                ChangedEventArgs args = new ChangedEventArgs(this, old, CurrentHealth);
                _healthInvoker?.Invoke(this, args);
            }
        }
        private float hpFromAmount(float amount, ChangeMode changeMode) {
            float hp = amount;

            // Determine the actual amount of HP based on the HealthChangeMode
            switch (changeMode) {
                case ChangeMode.PercentCurrent:
                    hp = amount * CurrentHealth;
                    break;

                case ChangeMode.PercentMax:
                    hp = amount * MaxHealth;
                    break;
            }

            return hp;
        }
    }

}
