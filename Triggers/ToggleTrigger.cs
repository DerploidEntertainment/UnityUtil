using UnityEngine;
using UnityEngine.Events;

namespace Danware.Unity.Triggers {

    public class ToggleTrigger : MonoBehaviour {

        private bool _on;

        public bool AwakeState = false;
        public UnityEvent TurnedOn = new UnityEvent();
        public UnityEvent TurnedOff = new UnityEvent();

        private void Awake() => _on = AwakeState;

        public void TurnOn() {
            if (!_on) {
                _on = true;
                TurnedOn.Invoke();
            }
        }
        public void Toggle() {
            _on = !_on;
            (_on ? TurnedOn : TurnedOff).Invoke();
        }
        public void TurnOff() {
            if (_on) {
                _on = false;
                TurnedOff.Invoke();
            }
        }

    }

}
