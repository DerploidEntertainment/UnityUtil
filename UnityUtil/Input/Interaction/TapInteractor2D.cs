using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    public class TapInteractor2D : MonoBehaviour {

        public LayerMask InteractLayerMask;

        private void Update() {
            if (U.Input.touchCount == 1) {
                Ray ray = Camera.main.ScreenPointToRay(U.Input.touches[0].position);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, InteractLayerMask);
                hit.collider?.GetComponent<Interactable2D>()?.Interact();
            }
        }

    }

}
