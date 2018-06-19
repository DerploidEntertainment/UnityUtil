using System;
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
        public QuantityEvent Restored = new QuantityEvent();

        // API
        public static float ConvertAmount(float amount, ChangeMode fromChangeMode, ChangeMode toChangeMode, float currentAmount, float maxAmount) {
            // Get the "from" change amount as an Absolute amount
            float absChange;
            switch (fromChangeMode) {
                case ChangeMode.Absolute:       absChange = amount;            break;
                case ChangeMode.PercentCurrent: absChange = amount * currentAmount;    break;
                case ChangeMode.PercentMax:     absChange = amount * maxAmount; break;
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(fromChangeMode));
            }

            // Convert that amount to the "to" change amount
            float newChange;
            switch (toChangeMode) {
                case ChangeMode.Absolute:       newChange = amount; break;
                case ChangeMode.PercentCurrent: newChange = absChange / currentAmount; break;
                case ChangeMode.PercentMax:     newChange = absChange / maxAmount; break;
                default: throw new NotImplementedException(BetterLogger.GetSwitchDefault(toChangeMode));
            }

            return newChange;
        }

        public float Increase(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Cannot increase {this.GetHierarchyNameWithType()} by a negative amount!");

            return doChange(amount, changeMode);
        }
        public float Decrease(float amount, ChangeMode changeMode = ChangeMode.Absolute) {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Cannot decrease {this.GetHierarchyNameWithType()} by a negative amount!");

            return doChange(-amount, changeMode);
        }
        public float Change(float amount, ChangeMode changeMode = ChangeMode.Absolute) => doChange(amount, changeMode);
        public void FillCompletely() => doChange(MaxValue - Value, ChangeMode.Absolute);
        public void DepleteCompletely() => doChange(-Value, ChangeMode.Absolute);

        // HELPER FUNCTIONS
        private float doChange(float amount, ChangeMode changeMode) {
            if (amount == 0f)
                return 0f;

            // Change the Current Value
            float old = Value;
            float change = ConvertAmount(amount, changeMode, ChangeMode.Absolute, Value, MaxValue);
            Value = Mathf.Clamp(old + change, MinValue, MaxValue);

            // Raise Value Changed events, if a change actually occurred
            if (Value != old) {
                if (old == MinValue)
                    Restored.Invoke(MinValue, Value);
                Changed.Invoke(old, Value);
                if (Value == MaxValue)
                    FullyFilled.Invoke(old, MaxValue);
                if (Value == MinValue)
                    FullyDepleted.Invoke(old, MinValue);
            }

            // Return the amount that was leftover after performing the change
            float leftOver = Mathf.Abs((old + change) - Value);
            return leftOver;
        }

    }

}
