using UnityEngine;
using U = UnityEngine;

namespace UnityEngine.Input {

    [CreateAssetMenu(fileName = "key-code-input", menuName = "UnityUtil/Input/key-code-input")]
    public sealed class KeyCodeInput : StartStopInput {
        
        public KeyCode KeyCode;

        public override bool Started() => U.Input.GetKeyDown(KeyCode);
        public override bool Happening() => U.Input.GetKey(KeyCode);
        public override bool Stopped() => U.Input.GetKeyUp(KeyCode);

    }

}
