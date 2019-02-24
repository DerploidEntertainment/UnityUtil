using Sirenix.OdinInspector;
using System;

namespace UnityEngine {

    [Serializable]
    public class SimpleInspectorEvent : InspectorEvent<EventArgs> { }

    public class InspectorButtonTrigger : MonoBehaviour {
        public SimpleInspectorEvent Triggered;
        public void Trigger() => Triggered.Invoke(EventArgs.Empty);
    }

}
