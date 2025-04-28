using UnityEngine;

namespace UnityUtil.Inputs;

public abstract class StartStopInput : ScriptableObject
{
    public abstract bool Started();
    public abstract bool Happening();
    public abstract bool Stopped();
}
