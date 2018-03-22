using UnityEngine;

namespace UnityUtil.Input {

    public class StartInteractor2D : MonoBehaviour {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;

        private void Update() {
            if (Input.Started()) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
                hit.collider?.GetComponent<Interactable2D>()?.Interact();
            }
        }

    }

}
