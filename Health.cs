using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityUtil {

    public class Health : MonoBehaviour {

        // ABSTRACT DATA TYPES
        public enum ChangeMode {
            Absolute,
            PercentCurrent,
            PercentMax,
        }
        /// <summary>
        /// Type arguments are (float oldHealth, float newHealth)
        /// </summary>
        [Serializable]
        public class HealthEvent : UnityEvent<float, float> { }

        // INSPECTOR FIELDS
        public float CurrentHealth;
        public float MaxHealth;
        public HealthEvent FullyHealed = new HealthEvent();
        public HealthEvent HealthChanged = new HealthEvent();
        public HealthEvent Killed = new HealthEvent();

        // API INTERFACE
        public void Heal(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            Assert.IsTrue(amount >= 0, $"Cannot heal {this.GetHierarchyNameWithType()} by a negative amount!");
            doChangeHealth(amount, changeMode);
        }
        public void HealCompletely() => doChangeHealth(MaxHealth - CurrentHealth, ChangeMode.Absolute);
        public void Damage(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            Assert.IsTrue(amount >= 0, $"Cannot wound {this.GetHierarchyNameWithType()} by a negative amount!");
            doChangeHealth(-amount, changeMode);
        }
        public void Kill() => doChangeHealth(-CurrentHealth, ChangeMode.Absolute);

        // HELPER FUNCTIONS
        private void doChangeHealth(float amount, ChangeMode changeMode) {
            // Change the Current Health
            float old = CurrentHealth;
            float hp = amount;
            switch (changeMode) {
                case ChangeMode.PercentCurrent: hp = amount * CurrentHealth; break;
                case ChangeMode.PercentMax:     hp = amount * MaxHealth;     break;
            }
            CurrentHealth = Mathf.Max(old + hp, 0f);

            // Raise Health Changed events, if a change actually occurred
            if (CurrentHealth != old)
                HealthChanged.Invoke(old, CurrentHealth);
            if (CurrentHealth == MaxHealth)
                FullyHealed.Invoke(old, MaxHealth);
            if (CurrentHealth == 0f)
                Killed.Invoke(old, 0f);
        }

    }

}
