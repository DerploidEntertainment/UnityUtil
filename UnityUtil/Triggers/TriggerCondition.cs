using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Triggers {

    public abstract class TriggerCondition : MonoBehaviour {

        public abstract bool IsConditionMet();
        public UnityEvent BecameTrue = new UnityEvent();
        public UnityEvent BecameFalse = new UnityEvent();

    }

}
