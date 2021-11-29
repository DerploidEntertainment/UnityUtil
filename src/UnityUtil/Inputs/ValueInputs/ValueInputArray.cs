using System.Linq;

namespace UnityEngine.Inputs {

    [CreateAssetMenu(fileName = "value-input-array", menuName = $"{nameof(UnityUtil)}/{nameof(UnityEngine.Inputs)}/{nameof(UnityEngine.Inputs.ValueInputArray)}")]
    public class ValueInputArray : ScriptableObject {
        // API INTERFACE
        public ValueInput[] Inputs;

        // API INTERFACE
        public int Length => Inputs.Length;

        public float[] Values() => Inputs.Select(i => i.Value()).ToArray();
        public bool AnyValue() => Inputs.Any(i => i.Value() != 0f);
        public bool AnyValuePositive() => Inputs.Any(i => i.Value() > 0f);
        public bool AnyValueNegative() => Inputs.Any(i => i.Value() < 0f);
        public int NumValues() => Inputs.Count(i => i.Value() != 0f);
        public int NumNegativeValues() => Inputs.Count(i => i.Value() < 0f);
        public int NumPosativeValues() => Inputs.Count(i => i.Value() > 0f);

        public float[] DiscreteValues() => Inputs.Select(i => i.DiscreteValue()).ToArray();
        public bool AnyDiscreteValue() => Inputs.Any(i => i.DiscreteValue() != 0f);
        public bool AnyDiscreteValuePositive() => Inputs.Any(i => i.DiscreteValue() > 0f);
        public bool AnyDiscreteValueNegative() => Inputs.Any(i => i.DiscreteValue() < 0f);
        public int NumDiscreteValues() => Inputs.Count(i => i.DiscreteValue() != 0f);
        public int NumNegativeDiscreteValues() => Inputs.Count(i => i.DiscreteValue() < 0f);
        public int NumPosativeDiscreteValues() => Inputs.Count(i => i.DiscreteValue() > 0f);

    }

}
