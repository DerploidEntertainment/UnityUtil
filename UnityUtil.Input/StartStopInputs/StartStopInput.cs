using UnityEngine;

namespace UnityUtil.Input {

    public abstract class StartStopInput : MonoBehaviour {
        
        public abstract bool Started();
        public abstract bool Happening();
        public abstract bool Stopped();

    }

}
