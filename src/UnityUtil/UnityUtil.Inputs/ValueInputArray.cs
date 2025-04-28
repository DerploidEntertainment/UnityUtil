using System.Linq;
using UnityEngine;

namespace UnityUtil.Inputs;

[CreateAssetMenu(fileName = "value-input-array", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inputs)}/{nameof(ValueInputArray)}")]
public class ValueInputArray : ScriptableObject
{
    public ValueInput[] Inputs = [];

    public int Length => Inputs.Length;

    public float[] Values() => [.. Inputs.Select(i => i.Value())];
    public bool AnyValue() => Inputs.Any(i => i.Value() != 0f);
    public bool AnyValuePositive() => Inputs.Any(i => i.Value() > 0f);
    public bool AnyValueNegative() => Inputs.Any(i => i.Value() < 0f);
    public int NumValues() => Inputs.Count(i => i.Value() != 0f);
    public int NumNegativeValues() => Inputs.Count(i => i.Value() < 0f);
    public int NumPosativeValues() => Inputs.Count(i => i.Value() > 0f);

    public float[] DiscreteValues() => [.. Inputs.Select(i => i.DiscreteValue())];
    public bool AnyDiscreteValue() => Inputs.Any(i => i.DiscreteValue() != 0f);
    public bool AnyDiscreteValuePositive() => Inputs.Any(i => i.DiscreteValue() > 0f);
    public bool AnyDiscreteValueNegative() => Inputs.Any(i => i.DiscreteValue() < 0f);
    public int NumDiscreteValues() => Inputs.Count(i => i.DiscreteValue() != 0f);
    public int NumNegativeDiscreteValues() => Inputs.Count(i => i.DiscreteValue() < 0f);
    public int NumPosativeDiscreteValues() => Inputs.Count(i => i.DiscreteValue() > 0f);

}
