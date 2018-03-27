﻿using UnityEngine;
using UnityUtil.Triggers;

namespace UnityUtil.Input {

    public class StartInteractor2D : BetterBehaviour {

        public StartStopInput Input;
        public float Range;
        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = raycast;
        }

        private void raycast() {
            if (Input.Started()) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, Range, InteractLayerMask);
                hit.collider?.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}