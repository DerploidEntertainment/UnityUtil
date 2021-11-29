using System;
using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class Interaction2DEventArgs : EventArgs {
        public RaycastHit2D HitInfo;
        public SimpleTrigger InteractedTrigger;
    }

    public class StartInteractor2D : Updatable {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;

        public event EventHandler<Interaction2DEventArgs> Interacted;

        protected override void Awake() {
            base.Awake();

            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycast;
        }

        private void raycast(float deltaTime) {
            if (Input.Started()) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
                if (hit.collider is not null) {
                    SimpleTrigger st = hit.collider.GetComponent<SimpleTrigger>();
                    st?.Trigger();
                    Interacted?.Invoke(this, new Interaction2DEventArgs() { HitInfo = hit, InteractedTrigger = st });
                }
            }
        }

    }

}
