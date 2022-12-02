using UnityEngine;

namespace UnityUtil.Inputs;

[CreateAssetMenu(fileName = "button-input", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inputs)}/{nameof(ButtonInput)}")]
public sealed class ButtonInput : StartStopInput
{
    public string ButtonName = "";

    public override bool Started() => Input.GetButtonDown(ButtonName);
    public override bool Happening() => Input.GetButton(ButtonName);
    public override bool Stopped() => Input.GetButtonUp(ButtonName);
}
