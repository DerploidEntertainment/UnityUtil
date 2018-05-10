using UnityEngine;

namespace UnityEngine.Input {

    public abstract class ValueInput : ScriptableObject {
        
        public abstract float Value();
        public abstract float DiscreteValue();
        
    }

}
