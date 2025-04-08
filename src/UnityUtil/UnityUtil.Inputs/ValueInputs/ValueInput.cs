using UnityEngine;

namespace UnityUtil.Inputs;

public abstract class ValueInput : ScriptableObject
{
    public abstract float Value();
    public abstract float DiscreteValue();
}
