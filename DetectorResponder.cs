using System;
using UnityEngine;

namespace Danware.Unity {

    public class DetectorResponseEventArgs : EventArgs {
        public DetectorResponseEventArgs(Collider target, DetectorBase detector, DetectorResponder responder) {
            Target = target;
            TargetComponent = target.GetComponent<PhysTarget>()?.TargetComponent;
            Detector = detector;
            Responder = responder;
        }

        public Collider Target { get; }
        public MonoBehaviour TargetComponent { get; }
        public DetectorBase Detector { get; }
        public DetectorResponder Responder { get; }
    }
    
    public class DetectorResponder : MonoBehaviour {

        private EventHandler<DetectorResponseEventArgs> _respondingInvoker;

        // API INTERFACE
        public void InitiateResponse(Collider target, DetectorBase detector) {
            var args = new DetectorResponseEventArgs(target, detector, this);
            _respondingInvoker.Invoke(this, args);
        }
        public event EventHandler<DetectorResponseEventArgs> Responding {
            add { _respondingInvoker += value; }
            remove { _respondingInvoker -= value; }
        }
    }

}
