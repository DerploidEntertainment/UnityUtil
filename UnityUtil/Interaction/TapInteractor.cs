﻿using UnityEngine.Triggers;

namespace UnityEngine.Inputs {

    public class TapInteractor : Updatable {

        public LayerMask InteractLayerMask;

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = tap;
        }

        private void tap() {
            if (Input.touchCount == 1) {
                Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, InteractLayerMask))
                    hitInfo.collider.GetComponent<SimpleTrigger>()?.Trigger();
            }
        }

    }

}