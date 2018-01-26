using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class CursorInteractor : MonoBehaviour {

        public LayerMask InteractLayerMask;
        public StartStopInput Input;

        private void Update() {
            if (Input.Started()) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                    hitInfo.collider.GetComponent<Interactable>()?.Interact();
            }
        }

    }

}
