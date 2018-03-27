using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    [CreateAssetMenu(fileName = "button-input", menuName = "UnityUtil/Input/button-input")]
    public sealed class ButtonInput : StartStopInput {
        
        public string ButtonName;

        public override bool Started() => U.Input.GetButtonDown(ButtonName);
        public override bool Happening() => U.Input.GetButton(ButtonName);
        public override bool Stopped() => U.Input.GetButtonUp(ButtonName);

    }

}
