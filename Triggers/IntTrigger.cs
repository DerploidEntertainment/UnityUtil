using System;
using UnityEngine;
using UnityEngine.Events;

namespace Danware.Unity.Triggers {

    public enum IntTriggerMode {
        /// <summary>
        /// Trigger event is raised every time the number reaches one of the specified values
        /// </summary>
        TargetValue,

        /// <summary>
        /// Trigger event is raised every time the encapsulated number is incremented the specified number of times
        /// </summary>
        Repeat,
    }

    [Serializable]
    public class IntEvent : UnityEvent<int> { }

    public class IntTrigger : MonoBehaviour {

        private int _number;
        private int _lastTriggerVal = 0;

        private void Awake() {
            _number = StartingValue;
            _lastTriggerVal = _number;
        }

        public IntTriggerMode Mode;
        [Tooltip("The encapsulated number is initialized to this value when the Scene starts.")]
        public int StartingValue = 0;
        [Tooltip("Trigger event is raised every time one of these values is reached.  Ignored if Mode is not TargetValue.")]
        public int[] TargetValues = new[] { 5 };
        [Tooltip("Trigger event is raised every time the encapsulated number is incremented by this amount.  Ignored if Mode is not Repeat.")]
        public int RepeatIncrementAmount;

        public IntEvent Triggered = new IntEvent();

        public void Increment() => checkValue(++_number);
        public void Decrement() => checkValue(--_number);

        private void checkValue(int number) {
            switch (Mode) {

                // If a target value has been reached, raise the trigger event
                case IntTriggerMode.TargetValue:
                    for (int n = 0; n < TargetValues.Length; ++n) {
                        if (number == TargetValues[n]) {
                            Triggered.Invoke(number);
                            break;
                        }
                    }
                    break;

                // If the value has incremented by the requested amount, raise the trigger event
                case IntTriggerMode.Repeat:
                    if (number - _lastTriggerVal == RepeatIncrementAmount) {
                        Triggered.Invoke(number);
                        _lastTriggerVal = number;
                    }
                    break;

                default:
                    throw new NotImplementedException($"Gah!  We didn't account for {nameof(IntTriggerMode)} {Mode}!");
            }

        }

    }

}
