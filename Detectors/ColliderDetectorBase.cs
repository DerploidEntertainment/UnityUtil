using System;

using UnityEngine;

namespace Danware.Unity {

    [RequireComponent(typeof(Collider))]
    public abstract class ColliderDetectorBase : MonoBehaviour {

        private EventHandler<ColliderDetectedEventArgs> _detectedInvoker;

        public event EventHandler<ColliderDetectedEventArgs> Detected {
            add { _detectedInvoker += value; }
            remove { _detectedInvoker -= value; }
        }

        protected void onDetected(Collider collider, MonoBehaviour target) {
            ColliderDetectedEventArgs args = new ColliderDetectedEventArgs(this, collider, target);
            _detectedInvoker?.Invoke(this, args);
        }

    }

    public class ColliderDetectedEventArgs : EventArgs {
        public ColliderDetectedEventArgs(ColliderDetectorBase detector, Collider collider, MonoBehaviour target) {
            Detector = detector;
            Collider = collider;
            Target = target;
        }
        public ColliderDetectorBase Detector { get; }
        public Collider Collider { get; }
        public MonoBehaviour Target { get; }
    }

}