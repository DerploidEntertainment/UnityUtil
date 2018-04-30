using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Input {

    public class StartInteractor : BetterBehaviour {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;
        public QueryTriggerInteraction QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycast;
        }

        private void raycast() {
            if (Input.Started()) {
                bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask, QueryTriggerInteraction);
                if (somethingHit)
                    hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}
