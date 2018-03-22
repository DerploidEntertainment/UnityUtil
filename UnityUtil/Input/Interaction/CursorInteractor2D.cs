using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class CursorInteractor2D : MonoBehaviour {

        public LayerMask InteractLayerMask;
        public StartStopInput Input;

        private void Update() {
            if (Input.Started()) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
                hit.collider?.GetComponent<Interactable2D>()?.Interact();
            }
        }

    }

}
