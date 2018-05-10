﻿using UnityEngine;

namespace UnityEngine.Input {

    public abstract class StartStopInput : ScriptableObject {
        
        public abstract bool Started();
        public abstract bool Happening();
        public abstract bool Stopped();

    }

}
