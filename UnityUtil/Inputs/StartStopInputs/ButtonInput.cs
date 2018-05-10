using UnityEngine;

namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "button-input", menuName = "UnityUtil/Input/button-input")]
    public sealed class ButtonInput : StartStopInput {
        
        public string ButtonName;

        public override bool Started() => Input.GetButtonDown(ButtonName);
        public override bool Happening() => Input.GetButton(ButtonName);
        public override bool Stopped() => Input.GetButtonUp(ButtonName);

    }

}
