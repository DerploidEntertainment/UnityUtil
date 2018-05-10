using UnityEngine;
using UnityEngine.Triggers;
using U = UnityEngine;

namespace UnityEngine.Input {

    public class TapInteractor : Updatable {

        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = tap;
        }

        private void tap() {
            if (U.Input.touchCount == 1) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.touches[0].position);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                    hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}
