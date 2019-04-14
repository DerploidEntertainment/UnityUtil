using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class LookAtInteractor2D : Updatable {

        private ToggleTrigger _trigger;

        public float Range;
        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }

        private void look(float deltaTime) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
            ToggleTrigger trigger = hit.collider?.GetComponent<ToggleTrigger>();
            if (trigger == null)
                _trigger?.TurnOff();
            else {
                if (_trigger == null) {
                    _trigger = trigger;
                    _trigger.TurnOn();
                }
                else if (trigger != _trigger) {
                    _trigger.TurnOff();
                    _trigger = trigger;
                    _trigger.TurnOn();
                }
            }
        }

    }

}
