using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Input {

    public class LookAtInteractor : BetterBehaviour {

        private ToggleTrigger _trigger;

        public float Range;
        public LayerMask InteractLayerMask;
        [Header("Gizmos")]
        [Tooltip("Should a ray Gizmo be drawn to indicate where this Component is looking?")]
        public bool DrawRay = true;
        [Tooltip("What color should the ray Gizmo be that indicates where this Component is looking?  Ignored if " + nameof(LookAtInteractor.DrawRay) + " is false.")]
        public Color RayColor = Color.black;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }
        private void OnDrawGizmos() {
            if (DrawRay) {
                Gizmos.color = RayColor;
                float range = (Range == Mathf.Infinity) ? 1000f : Range;
                Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.forward * range));
            }
        }

        private void look() {
            bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Range, InteractLayerMask);
            if (somethingHit) {
                ToggleTrigger trigger = hitInfo.collider.GetComponent<ToggleTrigger>();
                if (trigger != _trigger) {
                    _trigger?.TurnOff();
                    _trigger = trigger;
                    _trigger?.TurnOn();
                }
            }
            else {
                _trigger?.TurnOff();
                _trigger = null;
            }
        }

    }

}
