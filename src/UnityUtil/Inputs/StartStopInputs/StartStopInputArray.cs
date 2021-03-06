﻿using System.Linq;

namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "start-stop-input-array", menuName = "UnityUtil" + "/" + nameof(UnityEngine.Inputs) + "/" + nameof(UnityEngine.Inputs.StartStopInputArray))]
    public class StartStopInputArray : ScriptableObject {
        // API INTERFACE
        public StartStopInput[] Inputs;

        // INTERFACE
        public int Length => Inputs.Length;
        public bool[] Started() => Inputs.Select(i => i.Started()).ToArray();
        public bool AnyStarted() => Inputs.Any(i => i.Started());
        public int NumStarted() => Inputs.Count(i => i.Started());

        public bool[] Happening() => Inputs.Select(i => i.Happening()).ToArray();
        public bool AnyHappening() => Inputs.Any(i => i.Happening());
        public int NumHappening() => Inputs.Count(i => i.Happening());

        public bool[] Stopped() => Inputs.Select(i => i.Stopped()).ToArray();
        public bool AnyStopped() => Inputs.Any(i => i.Stopped());
        public int NumStopped() => Inputs.Count(i => i.Stopped());
    }

}
