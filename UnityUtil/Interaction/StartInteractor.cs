using System;
using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class InteractionEventArgs : EventArgs {
        public RaycastHit HitInfo;
        public SimpleTrigger InteractedTrigger;
    }

    public class StartInteractor : Updatable {

        // INSPECTOR INTERFACE
        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;
        public QueryTriggerInteraction QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        // API INTERFACE
        public event EventHandler<InteractionEventArgs> Interacted;

        // EVENT HANDLERS
        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycast;
        }

        private void raycast(float deltaTime) {
            if (Input.Started()) {
                bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask, QueryTriggerInteraction);
                if (somethingHit) {
                    SimpleTrigger st = hitInfo.collider.GetComponent<SimpleTrigger>();
                    st?.Trigger();
                    Interacted?.Invoke(this, new InteractionEventArgs() { HitInfo = hitInfo, InteractedTrigger = st });
                }
            }
        }

    }

}
