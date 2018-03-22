using UnityEngine;

namespace UnityUtil.Input {

    public class StartInteractor : MonoBehaviour {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;

        private void Update() {
            if (Input.Started()) {
                bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask);
                if (somethingHit)
                    hitInfo.collider.GetComponent<Interactable>()?.Interact();
            }
        }

    }

}
