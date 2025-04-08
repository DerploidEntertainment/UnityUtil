using System.Linq;
using UnityEngine;

namespace UnityUtil.Inputs;

[CreateAssetMenu(fileName = "start-stop-input-array", menuName = $"{nameof(UnityUtil)}/{nameof(UnityUtil.Inputs)}/{nameof(StartStopInputArray)}")]
public class StartStopInputArray : ScriptableObject
{
    public StartStopInput[] Inputs = [];

    public int Length => Inputs.Length;
    public bool[] Started() => Inputs.Select(i => i.Started()).ToArray();
    public bool AnyStarted() => Inputs.Any(i => i.Started());
    public int NumStarted() => Inputs.Count(i => i.Started());

    public bool[] Happening() => Inputs.Select(i => i.Happening()).ToArray();
    public bool AnyHappening() => Inputs.Any(i => i.Happening());
    public int NumHappening() => Inputs.Count(i => i.Happening());

    public bool[] Stopped() => Inputs.Select(i => i.Stopped()).ToArray();
    public bool AnyStopped() => Inputs.Any(i => i.Stopped());
    public int NumStopped() => Inputs.Count(i => i.Stopped());
}
