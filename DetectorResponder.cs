using System;
using UnityEngine;

namespace Danware.Unity {

    public class DetectorResponseEventArgs : EventArgs {
        public DetectorResponseEventArgs(DetectorResponder responder, DetectorBase detector, Collider targetCollider, MonoBehaviour target) {
            Responder = responder;
            Detector = detector;
            TargetCollider = targetCollider;
            Target = target;
        }

        public DetectorResponder Responder { get; }
        public DetectorBase Detector { get; }
        public Collider TargetCollider { get; }
        public MonoBehaviour Target { get; }
    }

    public abstract class DetectorResponder : MonoBehaviour {

        // HIDDEN FIELDS
        protected EventHandler<DetectorResponseEventArgs> _respondingInvoker;

        // API INTERFACE
        public event EventHandler<DetectorResponseEventArgs> Responding {
            add { _respondingInvoker += value; }
            remove { _respondingInvoker -= value; }
        }
        public abstract void BeginResponse(DetectorBase detector, Collider targetCollider, MonoBehaviour target);

    }

}
