using System;
using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class StartInteractor2D : Updatable {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;

        public event EventHandler<InteractionEventArgs> Interacted;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycast;
        }

        private void raycast() {
            if (Input.Started()) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
                if (hit.collider != null) {
                    SimpleTrigger st = hit.collider.GetComponent<SimpleTrigger>();
                    st?.Trigger();
                    Interacted?.Invoke(this, new InteractionEventArgs() { InteractedTrigger = st });
                }
            }
        }

    }

}
