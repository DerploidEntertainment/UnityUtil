using UnityEngine;

namespace UnityUtil.Input {

    public abstract class ValueInput : MonoBehaviour {
        
        public abstract float Value();
        public abstract float DiscreteValue();
        
    }

}
