using UnityEngine;
using UnityUtil.Triggers;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class CursorInteractor2D : Updatable {

        public LayerMask InteractLayerMask;
        public StartStopInput Input;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycastScreen;
        }

        private void raycastScreen() {
            if (Input.Started()) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
                hit.collider?.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}
