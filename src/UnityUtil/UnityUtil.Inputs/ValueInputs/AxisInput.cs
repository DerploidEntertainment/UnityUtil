using UnityEngine;

namespace UnityUtil.Inputs;

[CreateAssetMenu(fileName = "axis-input", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inputs)}/{nameof(AxisInput)}")]
public sealed class AxisInput : ValueInput
{
    public string AxisName = "";

    public override float DiscreteValue() => Input.GetAxisRaw(AxisName);
    public override float Value() => Input.GetAxis(AxisName);
}
