using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    [DisallowMultipleComponent]
    public sealed class KeyCodeInput : StartStopInput {
        
        public KeyCode KeyCode;

        public override bool Started() => U.Input.GetKeyDown(KeyCode);
        public override bool Happening() => U.Input.GetKey(KeyCode);
        public override bool Stopped() => U.Input.GetKeyUp(KeyCode);

    }

}
