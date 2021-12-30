namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "axis-input", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inputs)}/{nameof(UnityEngine.Inputs.AxisInput)}")]
    public sealed class AxisInput : ValueInput
    {
        public string AxisName = "";

        public override float DiscreteValue() => Input.GetAxisRaw(AxisName);
        public override float Value() => Input.GetAxis(AxisName);

    }

}
