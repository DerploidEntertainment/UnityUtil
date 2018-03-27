using UnityEngine;

namespace UnityUtil.Input {

    public abstract class ValueInput : ScriptableObject {
        
        public abstract float Value();
        public abstract float DiscreteValue();
        
    }

}
