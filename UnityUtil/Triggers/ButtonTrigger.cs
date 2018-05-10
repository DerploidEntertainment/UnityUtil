using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public class ButtonTrigger : MonoBehaviour {
        // INSPECTOR FIELDS
        public UnityEvent Triggered = new UnityEvent();
        public bool CanPress = true;
        public float RefractoryPeriod = 1f;  // seconds

        // API INTERFACE
        public void Use() {
            // Press the button if its refractory period has ended
            if (CanPress)
                press();
        }

        // HIDDEN FUNCTIONS
        private void press() {
            // Raise the trigger event
            this.Log($" pressed.");
            Triggered.Invoke();

            // Prevent the button from being pressed for the desired period
            CanPress = false;
            Invoke(nameof(reset), RefractoryPeriod);
        }
        private void reset() => CanPress = true;
    }

}
