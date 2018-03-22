using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Input {

    public class LookAtInteractor : BetterBehaviour {

        private ToggleTrigger _trigger;

        public float Range;
        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }

        private void look() {
            bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask);
            if (somethingHit) {
                ToggleTrigger trigger = hitInfo.collider.GetComponent<ToggleTrigger>();
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
            else
                _trigger?.TurnOff();
        }

    }

}
