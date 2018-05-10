namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "axis-input", menuName = "UnityUtil/Input/axis-input")]
    public sealed class AxisInput : ValueInput {

        public string AxisName;

        public override float DiscreteValue() => Input.GetAxisRaw(AxisName);
        public override float Value() => Input.GetAxis(AxisName);

    }

}
