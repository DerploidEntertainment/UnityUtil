using UnityEngine;
using UnityUtil.Triggers;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class TapInteractor2D : Updatable {

        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = tap;
        }

        private void tap() {
            if (U.Input.touchCount == 1) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.touches[0].position);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
                hit.collider?.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}
