using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine {

    public class ManagedQuantity : MonoBehaviour {

        // ABSTRACT DATA TYPES
        public enum ChangeMode {
            Absolute,
            PercentCurrent,
            PercentMax,
        }
        /// <summary>
        /// Type arguments are (float oldValue, float newValue)
        /// </summary>
        [Serializable]
        public class QuantityEvent : UnityEvent<float, float> { }

        // INSPECTOR FIELDS
        public float Value = 100f;
        public float MinValue = 0f;
        public float MaxValue = 100f;
        public QuantityEvent FullyFilled = new QuantityEvent();
        public QuantityEvent Changed = new QuantityEvent();
        public QuantityEvent FullyDepleted = new QuantityEvent();

        // API
        public float Increase(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Cannot increase {this.GetHierarchyNameWithType()} by a negative amount!");
            return doChange(amount, changeMode);
        }
        public float Decrease(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Cannot decrease {this.GetHierarchyNameWithType()} by a negative amount!");
            return doChange(-amount, changeMode);
        }
        public float Change(float amount, ChangeMode changeMode = ChangeMode.Absolute) => doChange(amount, changeMode);
        public void FillCompletely() => doChange(MaxValue - Value, ChangeMode.Absolute);
        public void DepleteCompletely() => doChange(-Value, ChangeMode.Absolute);

        // HELPER FUNCTIONS
        private float doChange(float amount, ChangeMode changeMode) {
            // Change the Current Value
            float old = Value;
            float change;
            switch (changeMode) {
                case ChangeMode.Absolute:       change = amount;            break;
                case ChangeMode.PercentCurrent: change = amount * Value;    break;
                case ChangeMode.PercentMax:     change = amount * MaxValue; break;
                default:
                    BetterLogger.LogError(BetterLogger.GetSwitchDefault(changeMode));
                    change = 0f;
                    break;
            }
            Value = Mathf.Clamp(old + change, MinValue, MaxValue);

            // Raise Value Changed events, if a change actually occurred
            if (Value != old)
                Changed.Invoke(old, Value);
            if (Value == MaxValue)
                FullyFilled.Invoke(old, MaxValue);
            if (Value == MinValue)
                FullyDepleted.Invoke(old, MinValue);

            // Return the amount that was leftover after performing the change
            float leftOver = Mathf.Abs((old + change) - Value);
            return leftOver;
        }

    }

}
