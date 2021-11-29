using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class LookAtInteractor : Updatable {

        // HIDDEN FIELDS
        private readonly IList<ToggleTrigger> _toggled = new List<ToggleTrigger>();
        private readonly HashSet<ToggleTrigger> _triggerBuffer = new();

        // INSPECTOR FIELDS
        [Header("Raycasting")]
        public float Range;
        public LayerMask InteractLayerMask;
        [Tooltip("If true, then all colliders within " + nameof(LookAtInteractor.Range) + " and on the " + nameof(LookAtInteractor.InteractLayerMask) + " will be interacted with (using the relatively expensive Physics.RaycastAll() method)  If false, then only " + nameof(LookAtInteractor.MaxInteractions) + " colliders will be interacted with.")]
        public bool InteractWithAllInRange = false;
        [Tooltip("The maximum number of colliders within " + nameof(LookAtInteractor.Range) + " and on the " + nameof(LookAtInteractor.InteractLayerMask) + " to interacted with.  If this value is 1, then Physics.Raycast() will be used to find colliders to interact with, otherwise the relatively expensive Physics.RaycastAll() will be used (with only the " + nameof(LookAtInteractor.MaxInteractions) + " closest colliders actually being interacted with).  This value can theoretically be zero, but that would make this " + nameof(Inputs.LookAtInteractor) + " kind of pointless!")]
        public uint MaxInteractions = 1;

        [Header("Gizmos")]
        [Tooltip("Should a ray Gizmo be drawn to indicate where this Component is looking?")]
        public bool DrawRay = true;
        [Tooltip("What color should the ray Gizmo be that indicates where this Component is looking?  Ignored if " + nameof(LookAtInteractor.DrawRay) + " is false.")]
        public Color RayColor = Color.black;

        // EVENT HANDLERS
        protected override void Awake() {
            base.Awake();

            RegisterUpdatesAutomatically = true;
            BetterUpdate = look;
        }
        protected override void OnDisable() {
            base.OnDisable();

            // Turn off all triggers being looked at
            for (int t = _toggled.Count - 1; t >= 0; --t) {
                _toggled[t].TurnOff();
                _toggled.RemoveAt(t);
            }
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        private void OnDrawGizmos() {
            if (DrawRay) {
                Gizmos.color = RayColor;
                float range = (Range == Mathf.Infinity) ? 1000f : Range;
                Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.forward * range));
            }
        }

        // HELPERS
        private void look(float deltaTime) {
            // Raycast for Colliders to look at
            RaycastHit[] hits = Array.Empty<RaycastHit>();
            if (InteractWithAllInRange || MaxInteractions > 1) {
                RaycastHit[] allHits = Physics.RaycastAll(transform.position, transform.forward, Range, InteractLayerMask);
                hits = allHits;
            }
            else if (MaxInteractions == 1) {
                bool somethingHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Range, InteractLayerMask);
                if (somethingHit)
                    hits = new[] { hit };
            }

            // Store the ToggleTriggers associated with those Colliders
            _triggerBuffer.Clear();
            for (int h = 0; h < hits.Length; ++h) {
                ToggleTrigger trigger = hits[h].collider.GetComponent<ToggleTrigger>();
                if (trigger is not null)
                    _triggerBuffer.Add(trigger);
            }

            // Turn on all triggers that haven't already been turned on
            foreach (ToggleTrigger t in _triggerBuffer) {
                if (!_toggled.Contains(t)) {
                    t.TurnOn();
                    _toggled.Add(t);
                }
            }

            // Turn off all triggers that are no longer being looked at
            for (int t = _toggled.Count - 1; t >= 0; --t) {
                if (!_triggerBuffer.Contains(_toggled[t])) {
                    _toggled[t].TurnOff();
                    _toggled.RemoveAt(t);
                }
            }
        }

    }

}
