using UnityEngine;
using UnityEngine.Events;

namespace Danware.Unity3D {

    public class Button : MonoBehaviour, Useable {
        // HIDDEN FIELDS
        private bool _toggled = false;

        // PUBLIC FIELDS
        public bool CanPress = true;
        public bool IsToggle = false;
        public float RefactoryPeriod = 1f;  // seconds
        public float ToggleEventRate = 2f;  // Events / second
        public UnityEvent FirstPress;
        public UnityEvent Pressed;

        // PUBLIC FUNCTIONS
        public void Use() {
            // Only continue if the Button is currently pressable
            if (!CanPress)
                return;

            // Press the Button according to whether it is a toggle-button
            if (IsToggle) {
                press();
                toggle();
            }
            else
                press();
        }

        // HIDDEN FUNCTIONS
        private void Awake() {
            reset();
        }
        private void press() {
            string pressedStr = (IsToggle ? "toggled " + (_toggled ? "OFF" : "ON") : "first pressed");
            Debug.LogFormat("Button {0} {1} in frame {2}", this.name, pressedStr, Time.frameCount);

            // Raise the Pressed events
            FirstPress.Invoke();
            if (!IsToggle || !_toggled)
                Invoke("onPressed", 0f);

            // Make the button un-pressable for the provided period
            CanPress = false;
            Invoke("reset", RefactoryPeriod);
        }
        private void toggle() {
            // Toggle the button
            _toggled = !_toggled;

            // Repeatedly raise the Pressed event if the button is toggled
            if (_toggled) {
                float togglePeriod = 1f / ToggleEventRate;
                InvokeRepeating("onPressed", togglePeriod, togglePeriod);
            }
            else
                CancelInvoke("onPressed");
        }
        private void reset() {
            CanPress = true;
        }
        private void onPressed() {
            Debug.LogFormat("Button {0} pressed in frame {1}", this.name, Time.frameCount);
            Pressed.Invoke();
        }
    }

}
