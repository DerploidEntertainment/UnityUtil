namespace Danware.Unity.Triggers {

    public class ToggleTrigger : TriggerCondition {

        private bool _on;

        public bool AwakeState = false;

        private void Awake() => _on = AwakeState;

        public void TurnOn() {
            if (!_on) {
                _on = true;
                BecameTrue.Invoke();
            }
        }
        public void Toggle() {
            _on = !_on;
            (_on ? BecameTrue : BecameFalse).Invoke();
        }
        public void TurnOff() {
            if (_on) {
                _on = false;
                BecameFalse.Invoke();
            }
        }

        public override bool IsConditionMet() => _on;

    }

}
