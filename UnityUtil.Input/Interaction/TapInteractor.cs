using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class TapInteractor : MonoBehaviour {

        public LayerMask InteractLayerMask;

        private void Update() {
            if (U.Input.touchCount == 1) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.touches[0].position);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                    hitInfo.collider.GetComponent<Interactable>()?.Interact();
            }
        }

    }

}
