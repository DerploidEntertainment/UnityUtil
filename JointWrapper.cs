using UnityEngine;
using System;

namespace Danware.Unity {

    public class JointWrapper : MonoBehaviour {
        // HIDDEN FIELDS
        private Joint _joint;
        private EventHandler _brokenInvoker;

        // API INTERFACE
        public event EventHandler Broken {
            add { _brokenInvoker += value; }
            remove { _brokenInvoker -= value; }
        }
        public void Break() {
            doBreak();
            Destroy(GetComponent<Joint>());
        }
        public J SetJoint<J>() where J : Joint {
            // Only allow the Joint to be set once
            if (Joint == null) {
                J j = gameObject.AddComponent<J>();
                Joint = j;
                return j;
            }
            else
                throw new InvalidOperationException("JointWrapper.SetJoint<> cannot be called if there is already an associated Joint!");
        }

        // EVENT HANDLERS
        private void OnJointBreak(float breakForce) => doBreak();

        // HELPER FUNCTIONS
        private Joint Joint {
            get {
                _joint = _joint ?? GetComponent<Joint>();
                return _joint;
            }
            set => _joint = value;
        }
        private void doBreak() {
            _brokenInvoker?.Invoke(this, EventArgs.Empty);
            Destroy(this);
        }

    }

}