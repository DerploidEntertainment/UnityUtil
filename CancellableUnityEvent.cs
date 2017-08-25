using UnityEngine.Events;
using System;

namespace Danware.Unity {

    [Serializable]
    public class CancellableUnityEvent : UnityEvent {
        public bool Cancel { get; set; }
        public new void Invoke() {
            Cancel = false;
            base.Invoke();
        }
    }

}
