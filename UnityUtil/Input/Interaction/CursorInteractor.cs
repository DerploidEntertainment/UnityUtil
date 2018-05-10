using UnityEngine;
using UnityEngine.Triggers;
using U = UnityEngine;

namespace UnityEngine.Input {

    public class CursorInteractor : Updatable {

        public LayerMask InteractLayerMask;
        public StartStopInput Input;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycastScreen;
        }

        private void raycastScreen() {
            if (Input.Started()) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                    hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}
