using System;

using UnityEngine;

namespace Danware.Unity {

    public class DetectedEventArgs : EventArgs {
        public DetectedEventArgs(DetectorBase detector, Collider collider, MonoBehaviour target) {
            Detector = detector;
            Collider = collider;
            Target = target;
        }
        public DetectorBase Detector { get; }
        public Collider Collider { get; }
        public MonoBehaviour Target { get; }
    }

    [RequireComponent(typeof(Collider))]
    public abstract class DetectorBase : MonoBehaviour {

        private EventHandler<DetectedEventArgs> _detectedInvoker;

        public event EventHandler<DetectedEventArgs> Detected {
            add { _detectedInvoker += value; }
            remove { _detectedInvoker -= value; }
        }

        public DetectorResponder Responder;

        protected void onDetected(Collider collider, MonoBehaviour target) {
            DetectedEventArgs args = new DetectedEventArgs(this, collider, target);
            _detectedInvoker?.Invoke(this, args);
        }

    }

}