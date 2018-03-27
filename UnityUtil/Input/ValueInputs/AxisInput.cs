using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Input {

    [CreateAssetMenu(fileName = "axis-input", menuName = "UnityUtil/Input/axis-input")]
    public sealed class AxisInput : ValueInput {

        public string AxisName;

        public override float DiscreteValue() => U.Input.GetAxisRaw(AxisName);
        public override float Value() => U.Input.GetAxis(AxisName);

    }

}
